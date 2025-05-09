using EXO.Networking.Common;
using EXO.WebServer.Server.Rooms;

namespace EXO.WebServer.Server.Routers
{
    public class RoomRouter : IMessageRouter
    {

        RoomManager roomManager;
        ClientManager clientManager;

        Dictionary<byte, Action<Packet, IClient>> PacketHandlers = new();


        public RoomRouter(RoomManager _roomManager, ClientManager _clientManager) 
        {
            roomManager = _roomManager;
            clientManager = _clientManager;

            PacketHandlers.Add((byte)PacketType.RequestHostRoom, this.HandleHostRoomPacket);
            PacketHandlers.Add((byte)PacketType.RequestJoinRoom, this.HandleJoinRoomPacket);
            PacketHandlers.Add((byte)PacketType.Custom, this.HandleCustomPacket);

        }

        public void RouteMessage(byte[] message, IClient from)
        {
            using (var packet = new Packet(message))
            {

                if (!PacketHandlers.ContainsKey(packet.Header))
                {
                    return;
                }

                var handler = PacketHandlers[packet.Header];
                handler?.Invoke(packet, from);
            }
        }

        private void HandleHostRoomPacket(Packet packet, IClient client)
        {
            var username = packet.ReadString();

            client.Name = username;

            var roomName = packet.ReadString();

            var presentRoom = roomManager.GetRoomByClient(client);

            // If the client was already in a different room...
            if (presentRoom != null)
            {
                presentRoom.room.RemoveClient(client);
            }

            var roomRec = roomManager.CreateRoom(client, roomName);

            using (var sendPacket = new Packet((byte)PacketType.ResponseHostRoom))
            {
                // Write the requesting clients ID...
                sendPacket.Write(client.ID);
                sendPacket.Write(roomRec.roomKey);

                client.Connection.Send(sendPacket.RawData);
            }
        }

        private void HandleJoinRoomPacket(Packet packet, IClient client)
        {
            var username = packet.ReadString();
            client.Name = username;

            var roomKey = packet.ReadString();
            roomManager.MoveRoom(client, roomKey);

            var roomRec = roomManager.rooms[roomKey];

            using (var sendPacket = new Packet((byte)PacketType.ResponseJoinRoom))
            {
                // Write the requesting clients ID...
                sendPacket.Write(client.ID);
                sendPacket.Write(roomRec.roomName);

                // Write all the currently connected clients...
                AppendUsers(sendPacket, roomRec.room);

                client.Connection.Send(sendPacket.RawData);
            }

        }

        private void AppendUsers(Packet packet, Room room)
        {
            packet.Write(room.clientRecords.Count);

            foreach (var record in room.clientRecords)
            {
                packet.Write(record.client);
            }
        }

        private void HandleCustomPacket(Packet packet, IClient client)
        {
            // Check if the user is owner or not...
            var room = roomManager.GetRoomByClient(client);

            if (room == null)
            {
                return;
            }

            // See if the client is the host...
            bool isHost = room.room.host.ID == client.ID;

            if (isHost)
            {
                // HOST PACKET LAYOUT
                // [HEADER][RECIEVING CLIENT ID][PAYLOAD]
                // PACKET CONTAINER : [HEADER][TO][PAYLOAD PACKET]

                // Grab who we are sending it too...
                var toSendToID = packet.ReadLong();

                // grab the client we are transmitting to...
                var clientToSendTo = room.room.clientRecords.First(c => c.client.ID == toSendToID);

                // Create the send packet...
                using (var sendPacket = packet.ReadPacket())
                {
                    // Transmit message...
                    clientToSendTo.client.Connection.Send(sendPacket.RawData);
                }

            }
            else // If they are not the host we want to send to the host...
            {
                // CLIENT PAYLOAD LAYOUT
                // [HEADER][PAYLOAD]


                // The Client we are sending to...
                var toSendTo = room.room.host;

                // Handler ID...
                var handlerID = packet.ReadInt();

                // Read the Rest of the packet into there...
                var rest = packet.ReadRest();

                using (var sendPacket = new Packet((byte)PacketType.Custom))
                {
                    sendPacket.Write(handlerID);
                    sendPacket.Write(client.ID);
                    sendPacket.Write(rest);

                    // Send the data to the host...
                    toSendTo.Connection.Send(sendPacket.RawData);
                }
            }
        }
    }
}
