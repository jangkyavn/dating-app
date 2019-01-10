using DatingApp.Data.Entities;
using DatingApp.Utilities.Helpers;
using System.Threading.Tasks;

namespace DatingApp.Application.IRepositories
{
    public interface IDatingRepository
    {
        void Add<T>(T entity) where T: class;
        void Delete<T>(T entity) where T : class;
        Task<PagedList<User>> GetUsers(UserParams userParams);
        Task<User> GetUser(int id);
        Task<Photo> GetPhoto(int id);
        Task<Photo> GetMainPhotoForUser(int userId);
        Task<bool> SaveAll();
    }
}
