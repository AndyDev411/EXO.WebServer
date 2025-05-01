
namespace EXO.WebServer.Server
{
    public class BaseClient : IClient
    {

        public long ClientID { get; set; }

        public BaseClient(long _clientID, IConnection _connection)
        {
            ClientID = _clientID;
            connection = _connection;
        }

        public IConnection Connection => connection;

        public CancellationTokenSource TokeSource { get; set; }

        private readonly IConnection connection;

        public async Task DisconnectAsync()
        {
            await connection.DisconnectAsync();
        }
    }
}
