using System;
using System.Threading.Tasks;

namespace EXO.Networking.Common
{
    public interface IConnection : IDisposable
    {

        public Task Send(byte[] toSend);

        public Task Send(string toSend);

        public Task<byte[]> Recieve();

        public Task<string> RecieveText();

        public Task DisconnectAsync();

        public bool Connected { get; }
    }
}


