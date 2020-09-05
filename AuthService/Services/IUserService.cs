using Auth.Models;
using System.Threading.Tasks;

namespace Auth.Services
{
    public interface IUserService
    {
        Task<User> RegisterUser(string username, string password, string email);
        Task<User> GetUser(string username, string password);
        User GetUserInfo(string username);
        bool IsExistingUser(string username);
    }
}
