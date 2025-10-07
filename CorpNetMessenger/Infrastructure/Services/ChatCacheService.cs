using System.Collections.Concurrent;
using CorpNetMessenger.Domain.Interfaces.Services;
using Microsoft.Extensions.Caching.Memory;

namespace CorpNetMessenger.Infrastructure.Services
{
    public class ChatCacheService : IChatCacheService
    {
        private readonly IMemoryCache _cache;
        private readonly ConcurrentDictionary<string, ConcurrentBag<string>> _chatCacheKeys = new();
        private readonly ILogger<ChatCacheService> _logger;

        public ChatCacheService(IMemoryCache cache, ILogger<ChatCacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Удаляет все связанные с указанным чатом ключи из кэша
        /// </summary>
        /// <param name="chatId">Идентификатор чата, для которого требуется очистить кэш</param>
        public void InvalidateChatCache(string chatId)
        {
            if (string.IsNullOrWhiteSpace(chatId))
            {
                _logger.LogWarning("Попытка удаления кэша с нулевым или пустым chatId");
                return;
            }

            if (_chatCacheKeys.TryGetValue(chatId, out var keys))
            {
                foreach (var key in keys)
                {
                    _cache.Remove(key);
                }
                _chatCacheKeys.TryRemove(chatId, out _);
            }
        }

        /// <summary>
        /// Регистрирует новый ключ кэша для указанного чата
        /// </summary>
        /// <param name="chatId">Идентификатор чата, для которого добавляется ключ кэша</param>
        /// <param name="cacheKey">Ключ кэша, который необходимо зарегистрировать</param>
        public void RegisterCacheKey(string chatId, string cacheKey)
        {
            if (string.IsNullOrWhiteSpace(chatId) || string.IsNullOrWhiteSpace(cacheKey))
            {
                _logger.LogWarning("Попытка регистрации кэша с нулевым или пустым chatId или cacheKey");
                return;
            }

            _chatCacheKeys.AddOrUpdate(chatId,
            id => new ConcurrentBag<string> { cacheKey },
            (id, keys) =>
            {
                if (!keys.Contains(cacheKey)) keys.Add(cacheKey);
                return keys;
            });
        }
    }
}