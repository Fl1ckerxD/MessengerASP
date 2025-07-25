using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CorpNetMessenger.Domain.Interfaces.Services
{
    public interface IChatCacheService
    {
        void RegisterCacheKey(string chatId, string cacheKey);
        void InvalidateChatCache(string chatId);
    }
}