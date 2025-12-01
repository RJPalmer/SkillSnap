using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace SkillSnap_API.Services;

/// <summary>
/// Service for managing in-memory caching with convenient async support.
/// Provides methods to get, set, and remove cached items with configurable expiration.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a cached item by key.
    /// </summary>
    /// <typeparam name="T">The type of the cached item</typeparam>
    /// <param name="key">The cache key</param>
    /// <returns>The cached item, or null if not found</returns>
    T? Get<T>(string key);

    /// <summary>
    /// Sets a cached item with a specified expiration time.
    /// </summary>
    /// <typeparam name="T">The type of the item to cache</typeparam>
    /// <param name="key">The cache key</param>
    /// <param name="value">The value to cache</param>
    /// <param name="expirationMinutes">Cache expiration time in minutes (default: 30)</param>
    void Set<T>(string key, T value, int expirationMinutes = 30);

    /// <summary>
    /// Removes a cached item by key.
    /// </summary>
    /// <param name="key">The cache key</param>
    void Remove(string key);

    /// <summary>
    /// Removes multiple cached items by key pattern prefix.
    /// </summary>
    /// <param name="keyPrefix">The prefix to match cache keys</param>
    void RemoveByPattern(string keyPrefix);

    /// <summary>
    /// Gets a cached item or executes a factory function to generate and cache it.
    /// </summary>
    /// <typeparam name="T">The type of the cached item</typeparam>
    /// <param name="key">The cache key</param>
    /// <param name="factory">Factory function to generate the item if not cached</param>
    /// <param name="expirationMinutes">Cache expiration time in minutes (default: 30)</param>
    /// <returns>The cached or generated item</returns>
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, int expirationMinutes = 30);
}

/// <summary>
/// Implementation of ICacheService using IMemoryCache.
/// </summary>
public class CacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<CacheService> _logger;

    public CacheService(IMemoryCache memoryCache, ILogger<CacheService> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public T? Get<T>(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogWarning("Cache key is null or empty");
            return default;
        }

        var success = _memoryCache.TryGetValue(key, out T? value);
        
        if (success)
        {
            _logger.LogDebug("Cache hit for key: {Key}", key);
        }
        else
        {
            _logger.LogDebug("Cache miss for key: {Key}", key);
        }

        return value;
    }

    public void Set<T>(string key, T value, int expirationMinutes = 30)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogWarning("Cache key is null or empty, skipping set operation");
            return;
        }

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expirationMinutes),
            SlidingExpiration = TimeSpan.FromMinutes(5) // Reset expiration on access
        };

        _memoryCache.Set(key, value, cacheOptions);
        _logger.LogDebug("Set cache item with key: {Key}, expiration: {ExpirationMinutes} minutes", key, expirationMinutes);
    }

    public void Remove(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogWarning("Cache key is null or empty, skipping remove operation");
            return;
        }

        _memoryCache.Remove(key);
        _logger.LogDebug("Removed cache item with key: {Key}", key);
    }

    public void RemoveByPattern(string keyPrefix)
    {
        if (string.IsNullOrWhiteSpace(keyPrefix))
        {
            _logger.LogWarning("Cache key prefix is null or empty, skipping remove operation");
            return;
        }

        _logger.LogDebug("RemoveByPattern not fully implemented - consider tracking keys separately for pattern removal");
        // Note: IMemoryCache doesn't natively support pattern-based removal.
        // If needed, maintain a separate collection of keys or use Redis for this functionality.
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, int expirationMinutes = 30)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogWarning("Cache key is null or empty, executing factory without caching");
            return await factory();
        }

        if (_memoryCache.TryGetValue(key, out T? cachedValue))
        {
            _logger.LogDebug("Cache hit for key: {Key}", key);
            return cachedValue!;
        }

        _logger.LogDebug("Cache miss for key: {Key}, executing factory", key);
        var value = await factory();

        if (value != null)
        {
            Set(key, value, expirationMinutes);
        }

        return value;
    }
}
