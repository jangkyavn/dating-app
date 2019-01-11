using DatingApp.Application.IRepositories;
using DatingApp.Data;
using DatingApp.Data.Entities;
using DatingApp.Utilities.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.Application.Repositories
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

        public async Task<Like> GetLike(int userId, int recipientId)
        {
            return await _dataContext.Likes.FirstOrDefaultAsync(x => x.LikerId == userId && x.LikeeId == recipientId);
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

        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            var users = _dataContext.Users.Include(x => x.Photos)
                .OrderByDescending(x => x.LastActive).AsQueryable();

            users = users.Where(x => x.Id != userParams.UserId);

            users = users.Where(x => x.Gender == userParams.Gender);

            if (userParams.Likers)
            {
                var userLikers = await GetUserLikes(userParams.UserId, userParams.Likers); // Lấy số người like mình
                users = users.Where(x => userLikers.Contains(x.Id));
            }

            if (userParams.Likees)
            {
                var userLikees = await GetUserLikes(userParams.UserId, userParams.Likers); // Lấy số người mình like
                users = users.Where(x => userLikees.Contains(x.Id));
            }

            if (userParams.MinAge != 18 || userParams.MaxAge != 99)
            {
                var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
                var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

                users = users.Where(x => x.DateOfBirth >= minDob && x.DateOfBirth <= maxDob);
            }

            if (!string.IsNullOrEmpty(userParams.OrderBy))
            {
                switch (userParams.OrderBy)
                {
                    case "created":
                        users = users.OrderByDescending(x => x.Created);
                        break;
                    default:
                        users = users.OrderByDescending(x => x.LastActive);
                        break;
                }
            }

            return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
        }

        private async Task<IEnumerable<int>> GetUserLikes(int id, bool likers)
        {
            var user = await _dataContext.Users
                .Include(x => x.Likers)
                .Include(x => x.Likees)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (likers)
            {
                return user.Likers.Where(x => x.LikeeId == id).Select(x => x.LikerId);
            }
            else
            {
                return user.Likees.Where(x => x.LikerId == id).Select(x => x.LikeeId);
            }
        }

        public async Task<bool> SaveAll()
        {
            return await _dataContext.SaveChangesAsync() > 0;
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _dataContext.Messages.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams)
        {
            var messages = _dataContext.Messages
                .Include(x => x.Sender).ThenInclude(x => x.Photos)
                .Include(x => x.Recipient).ThenInclude(x => x.Photos)
                .AsQueryable();

            switch (messageParams.MessageContainer)
            {
                case "Inbox":
                    messages = messages.Where(x => x.RecipientId == messageParams.UserId && x.RecipientDeleted == false);
                    break;
                case "Outbox":
                    messages = messages.Where(x => x.SenderId == messageParams.UserId && x.SenderDeleted == false);
                    break;
                default:
                    messages = messages.Where(x => x.RecipientId == messageParams.UserId && x.RecipientDeleted == false && x.IsRead == false);
                    break;
            }

            messages = messages.OrderByDescending(x => x.MessageSent);
            return await PagedList<Message>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<Message>> GetMessageThread(int userId, int recipientId)
        {
            var messages = await _dataContext.Messages
               .Include(x => x.Sender).ThenInclude(x => x.Photos)
               .Include(x => x.Recipient).ThenInclude(x => x.Photos)
               .Where(x => x.RecipientId == userId && x.SenderId == recipientId && x.RecipientDeleted == false
                    || x.RecipientId == recipientId && x.SenderId == userId && x.SenderDeleted == false)
                .OrderByDescending(x => x.MessageSent)
                .ToListAsync();

            return messages;
        }
    }
}
