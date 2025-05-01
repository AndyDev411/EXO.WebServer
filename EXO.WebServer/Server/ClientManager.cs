namespace EXO.WebServer.Server
{
    public class ClientManager : IClientManager
    {


        private readonly ClientFactory clientFactory;

        public ClientManager(ClientFactory _clientFactory)
        {
            clientFactory = _clientFactory;
        }


        private static long currentConnectionID = 0;

        private static Dictionary<long, IClient> Clients = new Dictionary<long, IClient>();

        public IClient HandleConnection(IConnection connection)
        {
            var client = clientFactory.CreateClient(connection);
            Clients.Add(client.ClientID, client);
            return client;
        }

        private long GetClientID()
        {
            var id = currentConnectionID;
            currentConnectionID++;
            return id;
        }

        public async Task DisconnectClient(long clientID)
        {
            await Clients[clientID].DisconnectAsync();
        }

        public async Task RemoveClient(long clientID)
        {
            var client = Clients[clientID];

            if (client.Connection.Connected)
            {
                await DisconnectClient(clientID);
            }

            Clients.Remove(clientID);
        }
    }
}
