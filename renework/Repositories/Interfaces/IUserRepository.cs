// Repositories/Interfaces/IUserRepository.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using renework.MongoDB.Collections;

namespace renework.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<List<User>> GetAllAsync();
        Task<User> GetByIdAsync(string id);
        Task<User> GetByEmailAsync(string email);
        Task<User> GetByUsernameAsync(string email);
        Task CreateAsync(User user);
        Task UpdateAsync(string id, User userIn);
        Task RemoveAsync(string id);
    }
}
