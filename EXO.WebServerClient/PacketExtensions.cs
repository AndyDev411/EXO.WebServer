using EXO.Networking.Common;

namespace EXO.WebClient
{
    public static class PacketExtensions
    {
        public static void WriteClient(this Packet packet, ExoClient client)
        {
            packet.Write(client.ID);
            packet.Write(client.Name);
        }

        public static ExoClient ReadClient(this Packet packet)
        {
            var id = packet.ReadLong();
            var name = packet.ReadString();

            ExoClient client = new ExoClient() { ID = id, Name = name };
            return client;
        }
    }
}
