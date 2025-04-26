// Repositories/Interfaces/IBusinessDataRepository.cs
using System.Threading.Tasks;
using renework.MongoDB.Collections;

namespace renework.Repositories.Interfaces
{
    public interface IBusinessDataRepository
    {
        Task<BusinessData?> GetByUserIdAsync(string userId);
        Task CreateAsync(BusinessData data);
        Task UpdateAsync(string id, BusinessData data);
        Task DeleteByUserIdAsync(string userId);
        Task<List<BusinessData>> GetAllAsync();
    }
}
