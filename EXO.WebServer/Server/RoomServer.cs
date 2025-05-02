

using EXO.WebServer.Server.Rooms;

namespace EXO.WebServer.Server
{
    public class RoomServer : BaseServer
    {

        private readonly RoomManager roomManager;

        public RoomServer(IMessageRouter _messageRouter, ILogger<BaseServer> _logger, RoomManager _roomManager) : base(_messageRouter, _logger)
        {
            roomManager = _roomManager;
        }

        public override Task HandleClient(IClient client)
        {
            return base.HandleClient(client);
        }
    }
}
