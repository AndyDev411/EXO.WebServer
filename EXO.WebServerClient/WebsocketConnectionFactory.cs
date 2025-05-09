using EXO.Networking.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXO.WebClient
{
    public class WebsocketConnectionFactory : IConnectionFactory
    {

        private int readBufferSize = 4096;

        private string url;

        public WebsocketConnectionFactory(string url, int readBufferSize)
        { 
            this.url = url;
            this.readBufferSize = readBufferSize;
        }

        public IConnection CreateConnection()
        {
            return new ClientWebsocketConnection(url, readBufferSize);
        }
    }
}
