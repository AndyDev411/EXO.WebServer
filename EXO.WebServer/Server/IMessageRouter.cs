using EXO.WebServer.Server;

namespace EXO.WebServer
{
    public interface IMessageRouter
    {
        public void RouteMessage(byte[] message, IClient from);
    }

}
