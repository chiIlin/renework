// Repositories/Interfaces/IApplicationRepository.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using renework.MongoDB.Collections;

namespace renework.Repositories.Interfaces
{
    public interface IApplicationRepository
    {
        Task<List<Application>> GetAllAsync();
        Task<Application> GetByIdAsync(string id);
        Task CreateAsync(Application item);
        Task UpdateAsync(string id, Application itemIn);
        Task RemoveAsync(string id);
    }
}
