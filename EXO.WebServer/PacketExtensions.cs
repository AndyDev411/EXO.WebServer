using EXO.Networking.Common;
using EXO.WebServer.Server;

namespace EXO.WebServer
{
    public static class PacketExtensions
    {
        public static void Write(this Packet packet, IClient client)
        { 
            packet.Write(client.ID);
            packet.Write(client.Name);
        }
    }
}
