// Repositories/CourseRepository.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using renework.MongoDB.Collections;
using renework.Repositories.Interfaces;

namespace renework.Repositories
{
    public class CourseRepository : ICourseRepository
    {
        private readonly IMongoCollection<Course> _courses;
        public CourseRepository(IMongoCollection<Course> courses) => _courses = courses;

        public async Task<List<Course>> GetAllAsync() =>
            await _courses.Find(_ => true).ToListAsync();

        public async Task<Course> GetByIdAsync(string id) =>
            await _courses.Find(c => c.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Course course) =>
            await _courses.InsertOneAsync(course);

        public async Task UpdateAsync(string id, Course courseIn) =>
            await _courses.ReplaceOneAsync(c => c.Id == id, courseIn);

        public async Task RemoveAsync(string id) =>
            await _courses.DeleteOneAsync(c => c.Id == id);
    }
}
