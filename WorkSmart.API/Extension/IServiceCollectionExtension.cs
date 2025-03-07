using Microsoft.EntityFrameworkCore;
using WorkSmart.Application.Mapper;
using WorkSmart.Application.Services;
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
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IJobRepository, JobRepository>();
            services.AddScoped<JobService>();
            services.AddAutoMapper(typeof(JobProfile));
            services.AddAutoMapper(typeof(NotificationProfile));
            services.AddAutoMapper(typeof(AccountProfile));
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<NotificationService>();
            services.AddScoped<ICandidateRepository, CandidateRepository>();
            services.AddScoped<CandidateProfile>();
            services.AddScoped<CandidateService>();
            services.AddScoped<EmployerService>();
            services.AddScoped<AdminService>();
            services.AddScoped<CloudinaryService>();
            services.AddTransient<ISendMailService, SendMailService>();
            services.AddTransient<ITokenRepository, TokenService>();
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddAutoMapper(typeof(CandidateProfile));

            services.AddScoped<TagService>();
            services.AddScoped<ITagRepository, TagRepository>();

            services.AddScoped<IPersonalMessageRepository, PersonalMessageRepository>();

            services.AddScoped<IPackageRepository, PackageRepository>();
            services.AddScoped<PackageService>();
            services.AddAutoMapper(typeof(PackageProfile));
            services.AddMemoryCache();

            services.AddSignalR();
            return services;
        }
    }
}
