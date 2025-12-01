using System;
using System.Threading.Tasks;
using SkillSnap_API.Services;

namespace SkillSnap_API_Test.Utils
{
    // Simple test cache implementation that does not persist items and always executes the factory.
    // This avoids behavior differences from IMemoryCache during unit/integration tests.
    public class TestCacheService : ICacheService
    {
        public T? Get<T>(string key) => default;

        public void Set<T>(string key, T value, int expirationMinutes = 30) { }

        public void Remove(string key) { }

        public void RemoveByPattern(string keyPrefix) { }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, int expirationMinutes = 30)
        {
            return await factory();
        }
    }
}
