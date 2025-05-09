using EXO.Networking.Common;

namespace EXO.WebServer.Server.Routers
{
    public class EchoRouter : IMessageRouter
    {

        private const string WHO_AM_I_COMMAND = "WHO AM I";

        public void RouteMessage(byte[] message, IClient from)
        {

            var msg = System.Text.Encoding.UTF8.GetString(message);

            if (msg.ToUpper() == WHO_AM_I_COMMAND)
            {
                from.Connection.Send($"You are client: {from.ID}");
                return;
            }

            from.Connection.Send(message);
        }
    }
}
