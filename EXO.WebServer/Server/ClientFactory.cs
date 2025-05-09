using EXO.Networking.Common;

namespace EXO.WebServer.Server
{
    public class ClientFactory
    {

        private static long currentClientID = 0;

        public IClient CreateClient(IConnection _connection)
        {
            return new BaseClient(GetNexID(), _connection) { };
        }

        private long GetNexID()
        {
            var id = currentClientID;
            currentClientID++;
            return id;
        }
    }
}
