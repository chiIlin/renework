// Repositories/UserRepository.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using renework.MongoDB.Collections;
using renework.Repositories.Interfaces;

namespace renework.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> _users;
        public UserRepository(IMongoCollection<User> users) => _users = users;

        public async Task<List<User>> GetAllAsync() =>
            await _users.Find(_ => true).ToListAsync();

        public async Task<User> GetByIdAsync(string id) =>
            await _users.Find(u => u.Id == id).FirstOrDefaultAsync();

        public async Task<User> GetByUsernameAsync(string username) =>
            await _users.Find(u => u.Username == username).FirstOrDefaultAsync();
        public async Task<User> GetByEmailAsync(string email) =>
            await _users.Find(u => u.Email == email).FirstOrDefaultAsync();

        public async Task CreateAsync(User user) =>
            await _users.InsertOneAsync(user);

        public async Task UpdateAsync(string id, User userIn) =>
            await _users.ReplaceOneAsync(u => u.Id == id, userIn);

        public async Task RemoveAsync(string id) =>
            await _users.DeleteOneAsync(u => u.Id == id);
    }
}
