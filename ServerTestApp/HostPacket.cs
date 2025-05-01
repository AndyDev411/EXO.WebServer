using EXO.WebServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerTestApp
{
    public class HostPacket : Packet
    {

        private long to;

        public HostPacket(PacketType packetType, long _to) : base((byte)packetType)
        {

            to = _to;
            Write(to);
            
        }
    }
}
