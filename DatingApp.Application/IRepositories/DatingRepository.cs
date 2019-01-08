using DatingApp.Application.Repositories;
using DatingApp.Data;
using DatingApp.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.Application.IRepositories
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _dataContext;

        public DatingRepository(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public void Add<T>(T entity) where T : class
        {
            _dataContext.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _dataContext.Remove(entity);
        }

        public async Task<Photo> GetMainPhotoForUser(int userId)
        {
            return await _dataContext.Photos.Where(x => x.UserId == userId).FirstOrDefaultAsync(x => x.IsMain);
        }

        public async Task<Photo> GetPhoto(int id)
        {
            var photo = await _dataContext.Photos.FirstOrDefaultAsync(x => x.Id == id);

            return photo;
        }

        public async Task<User> GetUser(int id)
        {
            var user = await _dataContext.Users.Include(x => x.Photos).FirstOrDefaultAsync(x => x.Id == id);

            return user;
        }

        public async Task<IEnumerable<User>> GetUsers()
        {
            var users = await _dataContext.Users.Include(x => x.Photos).ToListAsync();

            return users;
        }

        public async Task<bool> SaveAll()
        {
            return await _dataContext.SaveChangesAsync() > 0;
        }
    }
}
