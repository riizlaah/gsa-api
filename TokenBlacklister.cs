using Microsoft.Extensions.Caching.Memory;

namespace gsa_api
{
    public class TokenBlacklister
    {
        private readonly IMemoryCache cache;

        public TokenBlacklister(IMemoryCache cache)
        {
            this.cache = cache;
        }

        public void BlacklistToken(string TokenId)
        {
            cache.Set($"blacklist_{TokenId}", true);
        }

        public bool IsTokenBlacklisted(string tokenId)
        {
            return cache.TryGetValue($"blacklist_{tokenId}", out _);
        }
    }
}
