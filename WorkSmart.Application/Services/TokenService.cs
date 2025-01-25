using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSmart.Core.Interface;
using WorkSmart.Repository;

namespace WorkSmart.Application.Services
{
    public class TokenService : ITokenRepository
    {
        private readonly WorksmartDBContext _context;
        private readonly IDistributedCache _cache;

        public TokenService(WorksmartDBContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<bool> IsTokenUsedAsync(string token)
        {
            return await _cache.GetStringAsync(token) != null;
        }

        public async Task MarkTokenAsUsedAsync(string token)
        {
            // Lưu trữ token vào cache với thời gian hết hạn ngắn
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            };
            await _cache.SetStringAsync(token, "used", options);
        }
    }
}
