

namespace EXO.WebServer.Server
{
    public class BaseClient : IClient
    {

        public event Action<IClient> OnClientDisconnectEvent;

        public long ClientID { get; set; }

        public IConnection Connection => connection;

        /// <summary>
        /// Basic constructor.
        /// </summary>
        /// <param name="_clientID"> The ID of this client. </param>
        /// <param name="_connection"> This clients connection object. </param>
        public BaseClient(long _clientID, IConnection _connection)
        {
            ClientID = _clientID;
            connection = _connection;
        }

        public async Task ForceDisconnectAsync()
        {
            NotifyClientDisconnected();
            await connection.DisconnectAsync();
        }

        public void NotifyClientDisconnected()
        {
            OnClientDisconnectEvent?.Invoke(this);
        }

        private readonly IConnection connection;


        

    }
}
