
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using HotelReservationSystemAPI.Infrastructure.RedisCacheServie;
using HotelReservationSystemAPI.Application.Interface;

namespace HotelReservationSystemAPI.Infrastructure.MiddleWare
{
    /// <summary>
    /// This ratelimiter class uses the token bucket alogrithm to thrott users base on their ip address and user id
    /// </summary>
    public class RedisTokenBucketRateLimiter : IRateLimiter
    {
        private readonly RedisCacheService _cacheService;
        private readonly ILogger<RedisTokenBucketRateLimiter> _logger;

        private readonly int _maxTokens;
        private readonly double _refillRatePerSecond;
        private readonly TimeSpan _expiry;

        public RedisTokenBucketRateLimiter(
            RedisCacheService cacheService,
            ILogger<RedisTokenBucketRateLimiter> logger,
            int maxTokens = 20,
            double refillRatePerSecond = 20,
            TimeSpan? expiry = null)
        {
            _cacheService = cacheService;
            _logger = logger;
            _maxTokens = maxTokens;
            _refillRatePerSecond = refillRatePerSecond;
            _expiry = expiry ?? TimeSpan.FromSeconds(5);
        }

        public async Task<bool> AllowRequestAsync(string userId, string ipAddress)
        {
            string key = $"ratelimit:{userId}:{ipAddress}";

            var bucket = await _cacheService.GetAsync<TokenBucketState>(key)
                         ?? new TokenBucketState { Tokens = _maxTokens, LastRefill = DateTime.UtcNow };

            double elapsedSeconds = (DateTime.UtcNow - bucket.LastRefill).TotalSeconds;
            double refill = elapsedSeconds * _refillRatePerSecond;

            bucket.Tokens = Math.Min(_maxTokens, bucket.Tokens + refill);
            bucket.LastRefill = DateTime.UtcNow;

            if (bucket.Tokens >= 1)
            {
                bucket.Tokens -= 1;
                await _cacheService.SetAsync(key, bucket, _expiry);
                return true;
            }

            _logger.LogWarning("Rate limit exceeded for user {UserId} from IP {IP}", userId, ipAddress);
            return false;
        }

        private class TokenBucketState
        {
            public double Tokens { get; set; }
            public DateTime LastRefill { get; set; }
        }
    }

}
