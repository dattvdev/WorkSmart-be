using Microsoft.EntityFrameworkCore;
using WorkSmart.Application.Mapper;
using WorkSmart.Core.Interface;
using WorkSmart.Repository;
using WorkSmart.Repository.Repository;

namespace WorkSmart.API.Extension
{
    public static class IServiceCollectionExtension
    {
        public static IServiceCollection AddScopeCollection(this IServiceCollection services, string? Connectionstring)
        {
            //Add extentions here
            services.AddDbContext<WorksmartDBContext>(options =>
            options.UseSqlServer(Connectionstring));
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddAutoMapper(typeof(NotificationProfile));
            return services;
        }
    }
}
