namespace EXO.WebServer.Server
{
    public interface IClient
    {
        public long ClientID { get; set; }

        public IConnection Connection { get; }

        public CancellationTokenSource TokeSource { get; set; }

        public Task DisconnectAsync();
    }
}
