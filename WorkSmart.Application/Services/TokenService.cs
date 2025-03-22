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

        public async Task<bool> ValidateOtpAsync(string email, string otp)
        {
            var storedOtp = await _cache.GetStringAsync(email);
            if (storedOtp == otp)
            {
                await _cache.RemoveAsync(email);
                return true;
            }
            return false;
        }

        public async Task SaveOtpAsync(string email, string otp)
        {
            await _cache.SetStringAsync(email, otp, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            });
        }

        public async Task SaveResetTokenAsync(string email, string resetToken, TimeSpan expiry)
        {
            string resetTokenKey = $"reset_token_{email}";

            await _cache.SetStringAsync(resetTokenKey, resetToken, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry
            });
        }

        public async Task<bool> ValidateResetTokenAsync(string email, string resetToken)
        {
            string resetTokenKey = $"reset_token_{email}";
            var storedToken = await _cache.GetStringAsync(resetTokenKey);

            return storedToken == resetToken;
        }

        public async Task RemoveResetTokenAsync(string email)
        {
            string resetTokenKey = $"reset_token_{email}";
            await _cache.RemoveAsync(resetTokenKey);
        }
    }
}
