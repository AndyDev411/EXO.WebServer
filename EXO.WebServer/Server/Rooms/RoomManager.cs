namespace EXO.WebServer.Server.Rooms
{
    public class RoomManager
    {
        public Dictionary<string, RoomRecord> rooms = new();

        public Dictionary<long, RoomRecord> clientRoomDict = new();

        public RoomManager(ClientManager _clientManager)
        {
            _clientManager.OnClientDisconnectEvent += OnClientDisconnectHandler;
        }

        private void OnClientDisconnectHandler(IClient client)
        {

            // Grab the room...
            var room = GetRoomByClient(client);

            // If the client is the host... Kill the room...
            if (room.room.host.ClientID == client.ClientID)
            {
                // Remove the host first...
                room.room.RemoveClient(client);
                // Destory the room...
                DestroyRoom(room.roomKey);
            }
            else
            {
                // Remove the client from the room...
                room?.room.RemoveClient(client);
            }
        }

        public RoomRecord CreateRoom(IClient _host, string _roomName)
        {
            var room = new Room(_host);
            var rec = new RoomRecord(Guid.NewGuid().ToString(), room, _roomName);
            rooms.Add(rec.roomKey, rec);

            SetRoom(_host, room);

            return rec;
        }

        public void DestroyRoom(string roomKey)
        {
            rooms[roomKey].room.KillRoom();
            rooms.Remove(roomKey);
        }

        public RoomRecord? GetRoomByClient(IClient client)
        {

            if (!clientRoomDict.ContainsKey(client.ClientID))
            { return null; }

            return clientRoomDict[client.ClientID];
        }

        public void MoveRoom(IClient client, Room room)
        {

            // Tell last room the client has left...
            var lastRoom = GetRoomByClient(client);
            lastRoom?.room.RemoveClient(client);

            // Move the client to the room...
            room.AddClient(client);

            // Set the value of the client room dict...
            SetRoom(client, room);
        }

        private void SetRoom(IClient client, Room room)
        {
            if (clientRoomDict.ContainsKey(client.ClientID))
            {
                clientRoomDict[client.ClientID] = rooms.Values.First(c => c.room == room);
            }
            else
            {
                clientRoomDict.Add(client.ClientID, rooms.Values.First(c => c.room == room));
            }
        }

        public void MoveRoom(IClient client, string roomKey)
        {
            MoveRoom(client, rooms[roomKey].room);
        }
        public record RoomRecord(string roomKey, Room room, string roomName);

        public IEnumerable<RoomRecord> GetRoomRecords()
            => rooms.Values;

        public RoomRecord GetRoom(string roomKey)
            => rooms[roomKey];
    }
}
