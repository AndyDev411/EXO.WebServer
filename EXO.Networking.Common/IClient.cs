using System;
using System.Threading.Tasks;

namespace EXO.Networking.Common
{
    public interface IClient : IDisposable
    {
        /// <summary>
        /// Event called when the client is disconnected.
        /// </summary>
        public event Action<IClient> OnClientDisconnectEvent;


        /// <summary>
        /// The name of the client.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Clients Identifier.
        /// </summary>
        public long ClientID { get; set; }

        /// <summary>
        /// The Connection for this client.
        /// </summary>
        public IConnection Connection { get; }

        /// <summary>
        /// Forces a disconnection of this client.
        /// </summary>
        /// <returns> Task. </returns>
        public Task ForceDisconnectAsync();

        /// <summary>
        /// Notifies this client that it is disconnected if it does not already know.
        /// </summary>
        public void NotifyClientDisconnected();
    }
}
