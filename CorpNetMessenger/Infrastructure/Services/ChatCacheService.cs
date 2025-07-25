using System.Collections.Concurrent;
using CorpNetMessenger.Domain.Interfaces.Services;
using Microsoft.Extensions.Caching.Memory;

namespace CorpNetMessenger.Infrastructure.Services
{
    public class ChatCacheService : IChatCacheService
    {
        private readonly IMemoryCache _cache;
        private readonly ConcurrentDictionary<string, List<string>> _chatCacheKeys = new();

        public ChatCacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public void InvalidateChatCache(string chatId)
        {
            if (_chatCacheKeys.TryGetValue(chatId, out var keys))
            {
                foreach (var key in keys)
                {
                    _cache.Remove(key);
                }
                _chatCacheKeys.TryRemove(chatId, out _);
            }
        }

        public void RegisterCacheKey(string chatId, string cacheKey)
        {
            _chatCacheKeys.AddOrUpdate(chatId,
            id => new List<string> { cacheKey },
            (id, keys) =>
            {
                if (!keys.Contains(cacheKey)) keys.Add(cacheKey);
                return keys;
            });
        }
    }
}