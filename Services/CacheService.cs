using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Polly;

namespace FansVoice.UserService.Services
{
    public interface ICacheService
    {
        Task<T> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? expirationTime = null);
        Task RemoveAsync(string key);
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expirationTime = null);
    }

    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly DistributedCacheEntryOptions _defaultOptions;
        private readonly IConfiguration _configuration;
        private readonly ICircuitBreakerService _circuitBreaker;
        private readonly ILogger<CacheService> _logger;

        public CacheService(
            IDistributedCache cache,
            IConfiguration configuration,
            ICircuitBreakerService circuitBreaker,
            ILogger<CacheService> logger)
        {
            _cache = cache;
            _configuration = configuration;
            _circuitBreaker = circuitBreaker;
            _logger = logger;

            var defaultExpiration = TimeSpan.FromMinutes(
                int.Parse(_configuration["Redis:DefaultExpirationMinutes"] ?? "10"));

            _defaultOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = defaultExpiration
            };
        }

        public async Task<T> GetAsync<T>(string key)
        {
            try
            {
                return await _circuitBreaker.ExecuteAsync(async () =>
                {
                    var value = await _cache.GetStringAsync(key);
                    return value == null ? default : JsonSerializer.Deserialize<T>(value);
                }, $"GetCache_{typeof(T).Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting value from cache for key: {Key}", key);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expirationTime = null)
        {
            try
            {
                var options = expirationTime.HasValue
                    ? new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expirationTime }
                    : _defaultOptions;

                var jsonValue = JsonSerializer.Serialize(value);
                await _circuitBreaker.ExecuteAsync(async () =>
                {
                    await _cache.SetStringAsync(key, jsonValue, options);
                    return true;
                }, $"SetCache_{typeof(T).Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting value in cache for key: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _circuitBreaker.ExecuteAsync(async () =>
                {
                    await _cache.RemoveAsync(key);
                    return true;
                }, "RemoveCache");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing value from cache for key: {Key}", key);
            }
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expirationTime = null)
        {
            var value = await GetAsync<T>(key);
            if (value != null) return value;

            value = await factory();
            if (value != null)
            {
                await SetAsync(key, value, expirationTime);
            }

            return value;
        }
    }
}