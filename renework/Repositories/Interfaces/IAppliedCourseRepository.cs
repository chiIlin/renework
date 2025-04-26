// Repositories/Interfaces/IAppliedCourseRepository.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using renework.MongoDB.Collections;

namespace renework.Repositories.Interfaces
{
    public interface IAppliedCourseRepository
    {
        Task<List<AppliedCourse>> GetAllAsync();
        Task<AppliedCourse> GetByIdAsync(string id);
        Task CreateAsync(AppliedCourse item);
        Task UpdateAsync(string id, AppliedCourse itemIn);
        Task RemoveAsync(string id);
    }
}
