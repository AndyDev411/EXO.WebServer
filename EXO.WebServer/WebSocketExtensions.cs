using System.Net.WebSockets;

namespace EXO.WebServer
{
    public static class WebSocketExtensions
    {

        /// <summary>
        /// The Default read buffer size for RecieveMessageAsync.
        /// </summary>
        private const int DEFAULT_READ_BUFFER_SIZE = 4096;

        /// <summary>
        /// Easy way to just recieve the entire whole next message.
        /// </summary>
        /// <param name="socket"> The WebSocket we will use to communicate. </param>
        /// <param name="bufferSize"> The size of the buffer. </param>
        /// <param name="cancellationToken"> The Cancellation token. </param>
        /// <returns> The array of bytes. </returns>
        /// <exception cref="InvalidOperationException"> Thrown if you are trying to read from a WebSocket that is not open. </exception>
        public static async Task<byte[]> RecieveMessageAsync(this WebSocket socket, int bufferSize = DEFAULT_READ_BUFFER_SIZE, CancellationToken cancellationToken = default)
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

        /// <summary>
        /// Simple way to send an array of bytes.
        /// </summary>
        /// <param name="socket"> The socket we are using to send. </param>
        /// <param name="toSend">  </param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task SendMessageAsync(this WebSocket socket, byte[] toSend, CancellationToken token = default)
        => await socket.SendAsync(new(toSend), WebSocketMessageType.Text, true, token);
    }
}
