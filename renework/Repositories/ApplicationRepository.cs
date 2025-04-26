// Repositories/ApplicationRepository.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using renework.MongoDB.Collections;
using renework.Repositories.Interfaces;

namespace renework.Repositories
{
    public class ApplicationRepository : IApplicationRepository
    {
        private readonly IMongoCollection<Application> _col;
        public ApplicationRepository(IMongoCollection<Application> col) => _col = col;

        public async Task<List<Application>> GetAllAsync() =>
            await _col.Find(_ => true).ToListAsync();

        public async Task<Application> GetByIdAsync(string id) =>
            await _col.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Application item) =>
            await _col.InsertOneAsync(item);

        public async Task UpdateAsync(string id, Application itemIn) =>
            await _col.ReplaceOneAsync(x => x.Id == id, itemIn);

        public async Task RemoveAsync(string id) =>
            await _col.DeleteOneAsync(x => x.Id == id);
    }
}
