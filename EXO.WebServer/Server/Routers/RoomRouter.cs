using EXO.WebServer.Server.Rooms;

namespace EXO.WebServer.Server.Routers
{
    public class RoomRouter : IMessageRouter
    {

        RoomManager roomManager;

        Dictionary<byte, Action<Packet, IClient>> PacketHandlers = new();


        public RoomRouter(RoomManager _roomManager) 
        {
            roomManager = _roomManager;

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
            var roomName = packet.ReadString();

            var presentRoom = roomManager.GetRoomByClient(client);

            // If the client was already in a different room...
            if (presentRoom != null)
            {
                presentRoom.room.RemoveClient(client);
            }

            var roomRec = roomManager.CreateRoom(client, roomName);

            var sendPacket = new Packet((byte)PacketType.ResponseHostRoom);
            sendPacket.Write(roomRec.roomKey);

            client.Connection.Send(sendPacket.RawData);
        }

        private void HandleJoinRoomPacket(Packet packet, IClient client)
        {
            var roomKey = packet.ReadString();
            roomManager.MoveRoom(client, roomKey);

            var roomRec = roomManager.rooms[roomKey];

            var sendPacket = new Packet((byte)PacketType.ResponseJoinRoom);
            sendPacket.Write(roomRec.roomName);

            client.Connection.Send(sendPacket.RawData);
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
            bool isHost = room.room.host.ClientID == client.ClientID;

            if (isHost)
            {
                // HOST PACKET LAYOUT
                // [HEADER][RECIEVING CLIENT ID][PAYLOAD]
                // PACKET CONTAINER : [HEADER][TO][PAYLOAD PACKET]

                // Grab who we are sending it too...
                var toSendToID = packet.ReadLong();

                // grab the client we are transmitting to...
                var clientToSendTo = room.room.clientRecords.First(c => c.client.ClientID == toSendToID);

                // Create the send packet...
                var sendPacket = packet.ReadPacket();

                // Transmit message...
                clientToSendTo.client.Connection.Send(sendPacket.RawData);
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

                var sendPacket = new Packet((byte)PacketType.Custom);
                sendPacket.Write(handlerID);
                sendPacket.Write(client.ClientID);
                sendPacket.Write(rest);

                // Send the data to the host...
                toSendTo.Connection.Send(sendPacket.RawData);

            }

            
        }
    }
}
