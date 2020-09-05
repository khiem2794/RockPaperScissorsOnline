using Auth.Data;
using Auth.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Auth.Services
{
    public class UserService : IUserService
    {
        private readonly AuthDbContext _dbContext;

        public UserService(AuthDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public async Task<User> RegisterUser(string username, string password, string email)
        {
            if (IsExistingUser(username)) return null;
            User newUser = new User
            {
                UserName = username,
                Password = password,
                Email = email,
                RegisteredAt = DateTime.Now,
            };
            _dbContext.Users.Add(newUser);
            await _dbContext.SaveChangesAsync();
            return newUser;
        }

        public async Task<User> GetUser(string username, string password)
        {
            var user = (await _dbContext.Users.ToListAsync()).Where(u => u.Password == password && u.UserName == username).FirstOrDefault();
            return user;
        }

        public bool IsExistingUser(string username)
        {
            var user = _dbContext.Users.AsQueryable().Where(u => u.UserName == username).FirstOrDefault();
            if (user != null) return true;
            return false;
        }

        public User GetUserInfo(string username)
        {
            var user = _dbContext.Users.AsQueryable().Where(u => u.UserName == username).FirstOrDefault();
            return user;
        }
    }
}
