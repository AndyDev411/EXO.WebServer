using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXO.WebServerClient
{

    [AttributeUsage(AttributeTargets.Method)]
    public class ClientMessageHandlerAttribute : Attribute
    {

        public int HandlerID { get; private set; }

        public ClientMessageHandlerAttribute(int _handlerID)
        {
            HandlerID = _handlerID;
        }

    }
}
