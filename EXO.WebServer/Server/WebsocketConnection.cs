
using System.Net.WebSockets;

namespace EXO.WebServer
{
    public class WebsocketConnection : IConnection
    {

        WebSocket socket;
        private CancellationTokenSource tokenSource = new();

        public WebsocketConnection(WebSocket _socket)
        { 
            socket = _socket;
        }

        public bool Connected => !tokenSource.Token.IsCancellationRequested;

        public async Task DisconnectAsync()
        {
            await tokenSource.CancelAsync();
        }

        public async Task<byte[]> Recieve()
        {
            var result = await socket.RecieveMessageAsync(cancellationToken: tokenSource.Token);
            return result;
        }

        public async Task<string> RecieveText()
        {
            var bytes = await Recieve();
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        public async Task Send(byte[] toSend)
        {
            await socket.SendMessageAsync(toSend, tokenSource.Token);
        }

        public async Task Send(string toSend)
        {
            await socket.SendMessageAsync(System.Text.Encoding.UTF8.GetBytes(toSend), tokenSource.Token);
        }
    }
}
