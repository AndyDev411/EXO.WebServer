
namespace EXO.WebServer.Server
{
    public interface IClientManager
    {
        Task DisconnectClient(long clientID);
        IClient HandleConnection(IConnection connection);
        Task RemoveClient(long clientID);
    }
}