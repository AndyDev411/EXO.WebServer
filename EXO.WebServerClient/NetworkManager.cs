using EXO.WebClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EXO.Networking.Common;
using System.Diagnostics;

namespace EXO.WebClient
{
    public class NetworkManager
    {

        /// <summary>
        /// URL to the WebSocket Server.
        /// </summary>
        public string URL { get; set; } = "wss://play.theboizgaming.com:8080/ws";

        /// <summary>
        /// Static method used for sending things to the relay Server. Can be used by either a Client or a Host.
        /// </summary>
        /// <param name="toSend"> The Packet that we want to send. </param>
        /// <param name="to"> The ClientID of the person we want to send it to. (Only used if NetworkManager is acting as host.) </param>
        public static void Send(Packet toSend, long to = 0)
            => NetworkManager.Instance.SendPacket(toSend, to);

        /// <summary>
        /// Static method use for sending things to the relay Server. Can only be used by a Host.
        /// </summary>
        /// <param name="packet"> Packet we want to send. </param>
        /// <param name="except"> If there is one client we do not want to broadcast to set this to their CliendID. </param>
        public static void Broadcast(Packet packet, long? except = null)
        {
            if (NetworkManager.Instance == null)
            {
                throw new Exception("NetworkManager Instance has not been created, prior to static calls.");
            }

            NetworkManager.Instance.ServerBroadcast(packet, except);
        } 

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
        public event EventHandler<ExoClient> OnClientJoin;

        /// <summary>
        /// Event that is called when a Client has left.
        /// </summary>
        public event EventHandler<ExoClient> OnClientLeft;

        /// <summary>
        /// Client WebSocket we will use to communicate to the server.
        /// </summary>
        private IConnection connection;

        /// <summary>
        /// Connection Factory used for managing the connection.
        /// </summary>
        private readonly IConnectionFactory connectionFactory;

        /// <summary>
        /// The local persons ID...
        /// </summary>
        public static long LocalID { get; private set; }

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
        public Dictionary<long, ExoClient> clients = new();

        /// <summary>
        /// Clients that are connected to the server.
        /// </summary>
        private Dictionary<long, ExoClient> Clients { get; } = new();

        public NetworkManager(IConnectionFactory connectionFactory) 
        {

            this.connectionFactory = connectionFactory;

            if (Instance == null)
            {
                Instance = this;
            }

            InitHandlers();
        }

        /// <summary>
        /// Starts the NetworkManager as a Host on the Remote Server.
        /// </summary>
        /// <param name="roomName"> The name of the room you would like to host. </param>
        /// <returns> True if it was successful and false if it was not. </returns>
        public bool StartHost(string roomName, string username)
        {
            if (!Connect())
            {
                return false;
            }

            try
            {
                using (var packet = new Packet((byte)PacketType.RequestHostRoom))
                {
                    packet.Write(username);
                    packet.Write(roomName);
                    RoomName = roomName;
                    PSend(packet);

                    IsHost = true;
                }

                return true;
            }
            catch (Exception exc)
            {
                return false;
            }

        }


        /// <summary>
        /// Starts the NetworkManager as aHost on the Remote Server.
        /// </summary>
        /// <param name="roomKey"> The Room Key we would like to query for and join. </param>
        /// <returns> True if it sucessfully started as a client, and false if not. </returns>
        public bool StartClient(string roomKey, string username)
        {
            if (!Connect())
            {
                return false; 
            }

            try
            {
                using (var packet = new Packet((byte)PacketType.RequestJoinRoom))
                {
                    packet.Write(username);
                    packet.Write(roomKey);
                    RoomKey = roomKey;
                    PSend(packet);
                }

                return true;
            }
            catch (Exception exc)
            {
                return false;
            }


        }

        /// <summary>
        /// Connects to the Server.
        /// </summary>
        /// <returns> True if it sucessfully conects to the server. </returns>
        private bool Connect()
        {
            try
            {
                connection = connectionFactory.CreateConnection();

                Task.Run(HandleMessages);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

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
            => connection.Send(packet.RawData).GetAwaiter().GetResult();

        public void SendPacket(Packet packet, long to = 0)
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
            using (var newPacket = new Packet(packet.Header))
            {
                newPacket.Write(to);
                newPacket.Write(packet);

                PSend(newPacket);
            }
        }

        public void ServerBroadcast(Packet packet, long? except = null)
        {

            if (IsClient)
            { throw new Exception("Cannot Use ServerBroadcast as a client..."); }

            var clientIDs = Clients.Values.Select(c => c.ID);
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

            Debug.WriteLine("Starting the handling of messages!");

            while (connection.Connected)
            {
                using (var packet = await Recieve())
                {

                    Debug.WriteLine("Recieved a packet!");

                    var header = (PacketType)packet.Header;

                    Debug.WriteLine($"Packed Header: {header}");

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
        }





        #region Known Handlers

        private void Handle_ResponseHostRoom(Packet packet)
        {
            //[HEADER][ROOM KEY]
            LocalID = packet.ReadLong();
            RoomKey = packet.ReadString();
            OnHostStart?.Invoke(this, this);
        }

        private void Handle_ResponseJoinedRoom(Packet packet)
        {
            //[HEADER][ROOM NAME]
            LocalID = packet.ReadLong();
            RoomName = packet.ReadString();

            // Read all the client...
            ReadClients(packet);


            OnClientStart?.Invoke(this, this);
        }

        private void ReadClients(Packet packet)
        {
            int count = packet.ReadInt();

            for (int i = 0; i < count; i++)
            {
                Handle_ClientJoinedRoom(packet);
            }

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
            var client = packet.ReadClient();
            Clients.Add(client.ID, client);
            OnClientJoin?.Invoke(this, client);
        }

        #endregion

        public async Task<Packet> Recieve()
            => new Packet(await connection.Recieve());

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

    }
}
