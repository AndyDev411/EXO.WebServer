using System.Net.WebSockets;

namespace EXO.WebServer
{
    public static class WebSocketExtensions
    {
        public static async Task<byte[]> RecieveMessageAsync(this WebSocket socket, int bufferSize = 4 * 1024, CancellationToken cancellationToken = default)
        {
            if (socket.State != WebSocketState.Open)
                throw new InvalidOperationException("WebSocket is not open.");

            var buffer = new byte[bufferSize];
            using var ms = new MemoryStream();

            WebSocketReceiveResult result;
            do
            {
                result = await socket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    cancellationToken);

                // if the client asked to close, you can choose to close or return what you've got
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    // gracefully close from server side too
                    await socket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Closing in response to client",
                        cancellationToken);
                    return ms.ToArray();
                }

                ms.Write(buffer, 0, result.Count);
            }
            while (!result.EndOfMessage);

            return ms.ToArray();
        }

        public static async Task SendMessageAsync(this WebSocket socket, byte[] toSend, CancellationToken token = default)
        {
            await socket.SendAsync(new(toSend), WebSocketMessageType.Text, true, token);
        }
    }
}
