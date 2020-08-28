using System.Threading.Tasks;

namespace PlayService.Hubs
{
    public interface IGameHub
    {
        Task MessageClient(Message msg);
    }
}
