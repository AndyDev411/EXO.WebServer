using EXO.WebClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EXO.Networking.Common;

namespace EXO.WebClient
{
    public class NetworkManager
    {

        /// <summary>
        /// URL to the WebSocket Server.
        /// </summary>
        public const string URL = "wss://play.theboizgaming.com:8080/ws";

        /// <summary>
        /// Event that is called when the Host has connected to the server and was successful.
        /// </summary>
        public event EventHandler<NetworkManager> OnHostStart;

        /// <summary>
        /// Event that is called when the Client joins a room successfully.
        /// </summary>
        public event EventHandler<NetworkManager> OnClientStart;

        /// <summary>
        /// Event that is called when a Client has joined.
        /// </summary>
        public event EventHandler<ClientInfo> OnClientJoin;

        /// <summary>
        /// Event that is called when a Client has left.
        /// </summary>
        public event EventHandler<ClientInfo> OnClientLeft;

        /// <summary>
        /// Client WebSocket we will use to communicate to the server.
        /// </summary>
        private ClientWebSocket socket = new();

        /// <summary>
        /// Singleton Instance.
        /// </summary>
        public static NetworkManager? Instance { get; set; }

        /// <summary>
        /// True if we are the host and false if we are not.
        /// </summary>
        public static bool IsHost { get; private set; }

        /// <summary>
        /// True if we are client and false if not.
        /// </summary>
        public static bool IsClient => !IsHost;

        /// <summary>
        /// The Key to the room we are currently in.
        /// </summary>
        public string RoomKey { get; private set; }

        /// <summary>
        /// The name of the room.
        /// </summary>
        public string RoomName { get; private set; }

        private Dictionary<int, MethodInfo> clientHandlers = new();
        private Dictionary<int, MethodInfo> hostHandlers = new();

        /// <summary>
        /// Clients that are connected to the server.
        /// </summary>
        private Dictionary<long, ClientInfo> Clients { get; } = new();

        public NetworkManager() 
        {
            if (Instance == null)
            {
                Instance = this;
            }

            InitHandlers();
        }

        /// <summary>
        /// Starts the local NetworkManager as a host.
        /// </summary>
        /// <param name="roomName"> The name of the room we want to host. </param>
        public void StartHost(string roomName)
        {
            Connect();

            var packet = new Packet((byte)PacketType.RequestHostRoom);
            packet.Write(roomName);
            RoomName = roomName;
            PSend(packet);

            IsHost = true;
        }

        /// <summary>
        /// Starts the local NetworkManager as a client.
        /// </summary>
        /// <param name="roomKey"> The key to the room we want to join. </param>
        public void StartClient(string roomKey)
        {
            Connect();

            var packet = new Packet((byte)PacketType.RequestJoinRoom);
            packet.Write(roomKey);
            RoomKey = roomKey;
            PSend(packet);
        }

        /// <summary>
        /// Connects to the server.
        /// </summary>
        private void Connect()
        {
            socket.ConnectAsync(new(URL), CancellationToken.None).Wait();

            Task.Run(HandleMessages);
        }

        /// <summary>
        /// Initializes all the handlers.
        /// </summary>
        private void InitHandlers()
        {
            var clientMethods = GetStaticMethodsWithAttribute<ClientMessageHandlerAttribute>();
            var hostMethods = GetStaticMethodsWithAttribute<HostMessageHandlerAttribute>();

            foreach (var clientMeth in clientMethods)
            {
                var atrib = clientMeth.GetCustomAttribute<ClientMessageHandlerAttribute>();

                if (atrib == null)
                { continue; }

                clientHandlers.Add(atrib.HandlerID, clientMeth);
            }

            foreach (var hostMeth in hostMethods)
            {
                var atrib = hostMeth.GetCustomAttribute<HostMessageHandlerAttribute>();

                if (atrib == null)
                { continue; }

                hostHandlers.Add(atrib.HandlerID, hostMeth);
            }
        }

        /// <summary>
        /// Sends a packet.
        /// </summary>
        /// <param name="packet"> The packet we want to use. </param>
        private void PSend(Packet packet)
        {
            socket.SendAsync(new(packet.RawData), WebSocketMessageType.Text, true, CancellationToken.None).Wait();
        }

        public void Send(Packet packet, long to = 0)
        {
            if (IsHost)
            {
                ServerSend(to, packet);
            }
            else
            {
                ClientSend(packet);
            }
           
        }

        public void ClientSend(Packet packet)
        {
            if (IsHost)
            {
                throw new Exception("Cannot use ClientSend as the host!");
            }

            PSend(packet);
        }

        public void ServerSend(long to, Packet packet)
        {
            // Create the Packet that contains the packet...
            var newPacket = new Packet(packet.Header);
            newPacket.Write(to);
            newPacket.Write(packet);

            PSend(newPacket);
        }

        public void ServerBroadcast(Packet packet, long? except = null)
        {

            if (IsClient)
            { throw new Exception("Cannot Use ServerBroadcast as a client..."); }

            var clientIDs = Clients.Values.Select(c => c.clientID);
            foreach (var clientID in clientIDs)
            {

                // PACKET CONTAINER : [HEADER][TO][PAYLOAD PACKET]
                if (except.HasValue && except.Value == clientID)
                { continue; }

                ServerSend(clientID, packet);

            }
        }

        /// <summary>
        /// Handles the messages.
        /// </summary>
        /// <returns> Task. </returns>
        private async Task HandleMessages()
        {
            while (socket.State == WebSocketState.Open)
            {
                var packet = await Recieve();

                var header = (PacketType)packet.Header;

                // Check if it is a custom header...
                if (header == PacketType.Custom)
                {

                    var handlerID = packet.ReadInt();


                    // This is only for customers...
                    if (IsHost)
                    {
                        long from = packet.ReadLong();
                        // [Header][HandlerID][ClientID]
                        var handler = hostHandlers[handlerID];
                        handler.Invoke(null, new object[] { packet, from });
                    }
                    else
                    {

                        clientHandlers[handlerID].Invoke(null, new object[] { packet });
                    }
                }
                else // This is a Known Header...
                {
                    switch (header)
                    {
                        case PacketType.ResponseHostRoom:
                            Handle_ResponseHostRoom(packet);
                            break;
                        case PacketType.ResponseJoinRoom:
                            Handle_ResponseJoinedRoom(packet);
                            break;
                        case PacketType.ClientJoinedRoom:
                            Handle_ClientJoinedRoom(packet);
                            break;
                        case PacketType.ClientLeftRoom:
                            Handle_ClientLeftRoom(packet);
                            break;
                    }
                }


            }
        }





        #region Known Handlers

        private void Handle_ResponseHostRoom(Packet packet)
        {
            //[HEADER][ROOM KEY]
            RoomKey = packet.ReadString();
            OnHostStart?.Invoke(this, this);
        }

        private void Handle_ResponseJoinedRoom(Packet packet)
        {
            //[HEADER][ROOM NAME]
            RoomName = packet.ReadString();
            OnClientStart?.Invoke(this, this);
        }

        private void Handle_ClientLeftRoom(Packet packet)
        {
            // [HEADER][ClientID]
            var id = packet.ReadLong();
            var rec = Clients[id];
            Clients.Remove(id);
            OnClientLeft?.Invoke(this, rec);
        }

        private void Handle_ClientJoinedRoom(Packet packet)
        {
            // [HEADER][ClientID]
            var id = packet.ReadLong();
            var rec = new ClientInfo(id, "");
            Clients.Add(id, rec);
            OnClientJoin?.Invoke(this, rec);
        }

        #endregion

        public async Task<Packet> Recieve(int bufferSize = 4 * 1024, CancellationToken cancellationToken = default)
        {

            if (socket.State != WebSocketState.Open)
                throw new InvalidOperationException("WebSocket is not open.");

            var buffer = new byte[bufferSize];
            using var ms = new MemoryStream();

            WebSocketReceiveResult result;
            do
            {
                result = await socket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    cancellationToken);

                // if the client asked to close, you can choose to close or return what you've got
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    // gracefully close from server side too
                    await socket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Closing in response to client",
                        cancellationToken);
                    return new Packet(ms.ToArray());
                }

                ms.Write(buffer, 0, result.Count);
            }
            while (!result.EndOfMessage);

            return new Packet(ms.ToArray());
        }

        public enum NetworkType
        {
            Host,
            Client
        }

        private static List<MethodInfo> GetStaticMethodsWithAttribute<TAttribute>() where TAttribute : Attribute
        {
            List<MethodInfo> methodsWithAttribute = new List<MethodInfo>();

            // Get all assemblies that are loaded in the current AppDomain
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                // Get all types in the assembly
                var types = assembly.GetTypes();

                foreach (var type in types)
                {
                    // Get all static methods of the type
                    var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

                    foreach (var method in methods)
                    {
                        // Check if the method has the specified attribute
                        if (method.GetCustomAttributes(typeof(TAttribute), false).Any())
                        {
                            methodsWithAttribute.Add(method);
                        }
                    }
                }
            }

            return methodsWithAttribute;
        }

        public record ClientInfo(long clientID, string name);
    }
}
