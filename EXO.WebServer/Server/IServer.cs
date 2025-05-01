namespace EXO.WebServer.Server
{
    public interface IServer
    {
        public Task HandleClient(IClient client);
    }
}
