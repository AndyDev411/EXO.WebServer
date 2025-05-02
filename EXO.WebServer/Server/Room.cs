using EXO.Networking.Common;

namespace EXO.WebServer.Server
{
    public class Room
    {

        public EventHandler<ClientRecord> onClientJoinedEvent;
        public EventHandler<ClientRecord> onClientLeaveEvent;

        public IClient host;

        public Room(IClient _host)
        {
            host = _host;
            var rec = new ClientRecord(_host, true);
            clientRecords.Add(rec);
        }

        public List<ClientRecord> clientRecords = new();

        public void AddClient(IClient _toAdd)
        {
            var rec = new ClientRecord(_toAdd, false);
            clientRecords.Add(rec);
            onClientJoinedEvent?.Invoke(this, rec);

            // Create the packet...
            using (var packet = new Packet((byte)PacketType.ClientJoinedRoom))
            {
                packet.Write(_toAdd.ClientID);

                // Tell everyone the client has disconnected...
                foreach (var client in clientRecords)
                {

                    // Do not send to self...
                    if (client.client == _toAdd)
                    { continue; }

                    client.client.Connection.Send(packet.RawData);
                }
            }
        }

        public void RemoveClient(IClient _toRemove)
        {
            if (clientRecords.Any(c => c.client == _toRemove))
            {
                var rec = clientRecords.First(c => c.client == _toRemove);
                clientRecords.Remove(rec);
                onClientLeaveEvent?.Invoke(this, rec);

                // Create the packet...
                using (var packet = new Packet((byte)PacketType.ClientLeftRoom))
                {
                    packet.Write(_toRemove.ClientID);

                    // Tell everyone the client has disconnected...
                    foreach (var client in clientRecords)
                    {
                        client.client.Connection.Send(packet.RawData);
                    }
                }
            }
        }

        public void KillRoom()
        {
            foreach (var client in clientRecords)
            { 
                client.client.ForceDisconnectAsync().Wait();
            }
        }

        public record ClientRecord(IClient client, bool isHost);
    }
}
