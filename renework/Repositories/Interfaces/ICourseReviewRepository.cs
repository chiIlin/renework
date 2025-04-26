// Repositories/Interfaces/ICourseReviewRepository.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using renework.MongoDB.Collections;

namespace renework.Repositories.Interfaces
{
    public interface ICourseReviewRepository
    {
        Task<List<CourseReview>> GetAllAsync();
        Task<CourseReview> GetByIdAsync(string id);
        Task CreateAsync(CourseReview item);
        Task UpdateAsync(string id, CourseReview itemIn);
        Task RemoveAsync(string id);
    }
}
