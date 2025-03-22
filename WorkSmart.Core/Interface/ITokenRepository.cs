using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkSmart.Core.Interface
{
    public interface ITokenRepository
    {
        Task<bool> IsTokenUsedAsync(string token);

        Task MarkTokenAsUsedAsync(string token);

        Task SaveOtpAsync(string email, string otp);

        Task<bool> ValidateOtpAsync(string email, string otp);

        Task SaveResetTokenAsync(string email, string resetToken, TimeSpan expiry);

        Task<bool> ValidateResetTokenAsync(string email, string resetToken);

        Task RemoveResetTokenAsync(string email);
    }
}
