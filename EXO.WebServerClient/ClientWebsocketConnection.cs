using EXO.Networking.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EXO.WebClient
{
    internal class ClientWebsocketConnection : IConnection
    {


        private readonly ClientWebSocket clientWebSocket = new();
        private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();
        private readonly int bufferSize;

        public ClientWebsocketConnection(string url, int bufferSize) 
        {
            clientWebSocket.ConnectAsync(new(url), CancellationToken.None).GetAwaiter().GetResult();
            this.bufferSize = bufferSize;
        }

        public bool Connected => clientWebSocket.State == WebSocketState.Open;

        public async Task DisconnectAsync()
        {
            await clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, tokenSource.Token);
            tokenSource.Cancel();
            tokenSource.Dispose();
        }

        public async Task<byte[]> Recieve()
        {
            if (clientWebSocket.State != WebSocketState.Open)
                throw new InvalidOperationException("WebSocket is not open.");

            var buffer = new byte[bufferSize];
            using var ms = new MemoryStream();

            WebSocketReceiveResult result;
            do
            {
                result = await clientWebSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    tokenSource.Token);

                // if the client asked to close, you can choose to close or return what you've got
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    // gracefully close from server side too
                    await clientWebSocket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Closing in response to client",
                        tokenSource.Token);
                    return ms.ToArray();
                }

                ms.Write(buffer, 0, result.Count);
            }
            while (!result.EndOfMessage && !tokenSource.Token.IsCancellationRequested);

            return ms.ToArray();
        }

        public async Task<string> RecieveText()
        {
            var bytes = await Recieve();
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        public async Task Send(byte[] toSend)
        {
            await clientWebSocket.SendAsync(new(toSend), WebSocketMessageType.Text, true, tokenSource.Token);
        }

        public async Task Send(string toSend)
        {
            await Send(System.Text.Encoding.UTF8.GetBytes(toSend));
        }

        public void Dispose()
        {
            tokenSource.Dispose();
            clientWebSocket.Dispose();
        }
    }
}
