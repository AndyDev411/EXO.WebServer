namespace EXO.WebServer
{
    public interface IConnection
    {

        public Task Send(byte[] toSend);

        public Task Send(string toSend);

        public Task<byte[]> Recieve();

        public Task<string> RecieveText();

        public Task DisconnectAsync();

        public bool Connected { get; }
    }
}
