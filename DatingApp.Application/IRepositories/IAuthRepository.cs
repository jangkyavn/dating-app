using DatingApp.Data.Entities;
using System.Threading.Tasks;

namespace DatingApp.Application.IRepositories
{
    public interface IAuthRepository
    {
        Task<User> Register(User user, string password);
        Task<User> Login(string username, string password);
        Task<bool> UserExists(string username);
    }
}
