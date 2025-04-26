// Repositories/Interfaces/ICourseRepository.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using renework.MongoDB.Collections;

namespace renework.Repositories.Interfaces
{
    public interface ICourseRepository
    {
        Task<List<Course>> GetAllAsync();
        Task<Course> GetByIdAsync(string id);
        Task CreateAsync(Course course);
        Task UpdateAsync(string id, Course courseIn);
        Task RemoveAsync(string id);
    }
}
