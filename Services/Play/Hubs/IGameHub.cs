using System.Threading.Tasks;

namespace Play.Hubs
{
    public interface IGameHub
    {
        Task MessageClient(Message msg);
    }
}
