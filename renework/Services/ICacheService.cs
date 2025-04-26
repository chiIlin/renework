// Services/ICacheService.cs
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace renework.Services
{
    public interface ICacheService
    {
        Task SetAsync<T>(string key, T item, TimeSpan? expiry = null);
        Task<T?> GetAsync<T>(string key);
        Task RemoveAsync(string key);
    }
}
