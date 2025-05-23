﻿
using EXO.Networking.Common;

namespace EXO.WebServer.Server
{
    public class BaseServer : IServer
    {

        IMessageRouter messageRouter;
        ILogger logger;

        public BaseServer(IMessageRouter _messageRouter, ILogger<BaseServer> _logger)
        { 
            messageRouter = _messageRouter;
            logger = _logger;
        }

        public virtual async Task HandleClient(IClient client)
        {

            using (var innerClient = client)
            {
                logger?.LogInformation($"{innerClient.ID} : Running Client Logic Loop...");
                while (innerClient.Connection.Connected)
                {
                    try
                    {
                        var message = await innerClient.Connection.Recieve();
                        messageRouter.RouteMessage(message, client);
                    }
                    catch
                    {
                        break;
                    }

                }

                // Let the client know it disconnected...
                client.NotifyClientDisconnected();
                logger?.LogInformation($"{innerClient.ID} : Has Disconnected...");
            }

        }
    }
}
