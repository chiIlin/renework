// Services/RedisCacheService.cs
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace renework.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        public RedisCacheService(IDistributedCache cache) =>
            _cache = cache;

        public async Task SetAsync<T>(string key, T item, TimeSpan? expiry = null)
        {
            var opts = new DistributedCacheEntryOptions();
            if (expiry.HasValue) opts.SetAbsoluteExpiration(expiry.Value);
            var json = JsonSerializer.Serialize(item);
            await _cache.SetStringAsync(key, json, opts);
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var json = await _cache.GetStringAsync(key);
            return json is null
                ? default
                : JsonSerializer.Deserialize<T>(json);
        }

        public Task RemoveAsync(string key) =>
            _cache.RemoveAsync(key);
    }
}
