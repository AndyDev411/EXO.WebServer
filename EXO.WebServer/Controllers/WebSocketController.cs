using EXO.WebServer.Server;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;

namespace EXO.WebServer.Controllers
{
    public class WebSocketController : Controller
    {

        private readonly ClientManager clientManager;
        private readonly IServer server;

        public WebSocketController( ClientManager _clientManager, IServer _server)
        { 
            clientManager = _clientManager;
            server = _server;
        }

        [Route("/ws")]
        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await HandleSocket(webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        private async Task HandleSocket(WebSocket socket)
        {
            // Create a connection...
            var connection = new WebsocketConnection(socket);
            var newClient = clientManager.HandleConnection(connection);
            await server.HandleClient(newClient);
        }

        private async Task Echo(WebSocket webSocket)
        {

            while (webSocket.State == WebSocketState.Open)
            {
                var buffer = new byte[4 * 1024];
                var seg = new ArraySegment<byte>(buffer);
                var res = await webSocket.ReceiveAsync(seg, CancellationToken.None);
                if (res.MessageType == WebSocketMessageType.Close)
                    break;


                await webSocket.SendAsync(seg[..res.Count], res.MessageType, res.EndOfMessage, CancellationToken.None);
            }


        }
    }
}
