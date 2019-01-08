using DatingApp.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DatingApp.Application.Repositories
{
    public interface IDatingRepository
    {
        void Add<T>(T entity) where T: class;
        void Delete<T>(T entity) where T : class;
        Task<IEnumerable<User>> GetUsers();
        Task<User> GetUser(int id);
        Task<Photo> GetPhoto(int id);
        Task<Photo> GetMainPhotoForUser(int userId);
        Task<bool> SaveAll();
    }
}
