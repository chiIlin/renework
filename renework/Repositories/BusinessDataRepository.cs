// Repositories/BusinessDataRepository.cs
using System.Threading.Tasks;
using MongoDB.Driver;
using renework.MongoDB.Collections;
using renework.MongoDB.Context;
using renework.Repositories.Interfaces;

namespace renework.Repositories
{
    public class BusinessDataRepository : IBusinessDataRepository
    {
        private readonly IMongoCollection<BusinessData> _col;

        public BusinessDataRepository(MongoDbContext ctx)
        {
            _col = ctx.BusinessData;
        }

        public async Task<BusinessData?> GetByUserIdAsync(string userId) =>
            await _col.Find(d => d.UserId == userId).FirstOrDefaultAsync();

        public async Task CreateAsync(BusinessData data) =>
            await _col.InsertOneAsync(data);

        public async Task UpdateAsync(string id, BusinessData data) =>
            await _col.ReplaceOneAsync(d => d.Id == id, data);

        public async Task DeleteByUserIdAsync(string userId) =>
            await _col.DeleteOneAsync(d => d.UserId == userId);
        public async Task<List<BusinessData>> GetAllAsync() =>
            await _col.Find(_ => true).ToListAsync();
    }
}
