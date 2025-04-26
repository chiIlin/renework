// Repositories/CourseReviewRepository.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using renework.MongoDB.Collections;
using renework.Repositories.Interfaces;

namespace renework.Repositories
{
    public class CourseReviewRepository : ICourseReviewRepository
    {
        private readonly IMongoCollection<CourseReview> _col;
        public CourseReviewRepository(IMongoCollection<CourseReview> col) => _col = col;

        public async Task<List<CourseReview>> GetAllAsync() =>
            await _col.Find(_ => true).ToListAsync();

        public async Task<CourseReview> GetByIdAsync(string id) =>
            await _col.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(CourseReview item) =>
            await _col.InsertOneAsync(item);

        public async Task UpdateAsync(string id, CourseReview itemIn) =>
            await _col.ReplaceOneAsync(x => x.Id == id, itemIn);

        public async Task RemoveAsync(string id) =>
            await _col.DeleteOneAsync(x => x.Id == id);
    }
}
