// Repositories/AppliedCourseRepository.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using renework.MongoDB.Collections;
using renework.Repositories.Interfaces;

namespace renework.Repositories
{
    public class AppliedCourseRepository : IAppliedCourseRepository
    {
        private readonly IMongoCollection<AppliedCourse> _col;
        public AppliedCourseRepository(IMongoCollection<AppliedCourse> col) => _col = col;

        public async Task<List<AppliedCourse>> GetAllAsync() =>
            await _col.Find(_ => true).ToListAsync();

        public async Task<AppliedCourse> GetByIdAsync(string id) =>
            await _col.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(AppliedCourse item) =>
            await _col.InsertOneAsync(item);

        public async Task UpdateAsync(string id, AppliedCourse itemIn) =>
            await _col.ReplaceOneAsync(x => x.Id == id, itemIn);

        public async Task RemoveAsync(string id) =>
            await _col.DeleteOneAsync(x => x.Id == id);
    }
}
