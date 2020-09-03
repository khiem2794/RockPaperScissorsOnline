using AuthService.Models;
using System.Threading.Tasks;

namespace AuthService.Services
{
    public interface IUserService
    {
        Task<User> RegisterUser(string username, string password, string email);
        Task<User> GetUser(string username, string password);
        User GetUserInfo(string username);
        bool IsExistingUser(string username);
    }
}
