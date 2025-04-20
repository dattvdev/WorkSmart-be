using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorkSmart.API.SignalRService;
using WorkSmart.Application.Mapper;
using WorkSmart.Application.Services;
using WorkSmart.Core.Dto.MailDtos;
using WorkSmart.Core.Interface;
using WorkSmart.Repository;
using WorkSmart.Repository.Repository;

namespace WorkSmart.API.Extension
{
    public static class IServiceCollectionExtension
    {
        public static IServiceCollection AddScopeCollection(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                                    ?? configuration["ConnectionStrings:DefaultConnection"];

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new Exception("❌ Không tìm thấy ConnectionString");
            }
            Console.WriteLine("🔗 ConnectionString: " + builder.Configuration.GetConnectionString("DefaultConnection"));

            Console.WriteLine("🔍--- Environment Variables ---");
            foreach (DictionaryEntry env in Environment.GetEnvironmentVariables())
            {
                Console.WriteLine($"{env.Key} = {env.Value}");
            }
            
            Console.WriteLine("🔍--- App Configuration Values ---");
            foreach (var kv in builder.Configuration.AsEnumerable())
            {
                if (!string.IsNullOrWhiteSpace(kv.Value)) // tránh log null
                    Console.WriteLine($"{kv.Key} = {kv.Value}");
            }


            services.AddDbContext<WorksmartDBContext>(options =>
                options.UseSqlServer(connectionString));

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new Exception("❌ Không tìm thấy ConnectionString");
            }

            services.AddDbContext<WorksmartDBContext>(options =>
                options.UseSqlServer(connectionString));
            //job
            services.AddScoped<IJobRepository, JobRepository>();
            services.AddScoped<JobService>();
            services.AddAutoMapper(typeof(JobProfile));
            //application
            services.AddScoped<IApplicationRepository, ApplicationRepository>();
            services.AddScoped<ApplicationService>();
            services.AddAutoMapper(typeof(ApplicationProfile));
            //account
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddTransient<ISendMailService, SendMailService>();
            services.AddTransient<ITokenRepository, TokenService>();
            services.AddAutoMapper(typeof(AccountProfile));
            //notification
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<NotificationService>();
            services.AddAutoMapper(typeof(NotificationProfile));
            services.AddScoped<SignalRNotificationService>();
            //notificationJobTag
            services.AddScoped<INotificationJobTagRepository, NotificationJobTagRepository>();
            services.AddScoped<NotificationJobTagService>();
            //candidate
            services.AddScoped<ICandidateRepository, CandidateRepository>();
            services.AddAutoMapper(typeof(CandidateProfile));
            services.AddScoped<CandidateService>();
            //employer
            services.AddScoped<EmployerService>();
            services.AddScoped<AdminService>();
            services.AddScoped<CloudinaryService>();
            //tag
            services.AddScoped<ITagRepository, TagRepository>();
            services.AddScoped<TagService>();
            //user
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<UserService>();
            services.AddAutoMapper(typeof(UserProfile));
            //message
            services.AddScoped<IPersonalMessageRepository, PersonalMessageRepository>();
            services.AddScoped<MessageService>();
            //subscription
            services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
            services.AddScoped<SubscriptionService>();
            services.AddAutoMapper(typeof(SubscriptionProfile));
            //package
            services.AddScoped<IPackageRepository, PackageRepository>();
            services.AddScoped<PackageService>();
            services.AddAutoMapper(typeof(PackageProfile));
            //cv
            services.AddScoped<ICVRepository, CVRepository>();
            services.AddScoped<OpenAIService>();
            services.AddScoped<CVService>();
            services.AddScoped<ICvParserService, CvParserService>();
            //mail
            services.AddScoped<SendMailService>();
            //report
            services.AddScoped<IReportRepository, ReportRepository>();
            services.AddScoped<ReportService>();
            services.AddAutoMapper(typeof(ReportProfile));           
            //favorite-job
            services.AddScoped<IFavoriteJobRepository,FavoriteJobRepository>();
            services.AddScoped<FavoriteJobService>();
            services.AddAutoMapper(typeof(FavoriteJobProfile));
            //payOS
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<TransactionService>();
            services.AddAutoMapper(typeof (TransactionProfile));
            //Background
            services.AddSingleton<JobNotificationBackgroundService>();
            services.AddHostedService(sp => sp.GetRequiredService<JobNotificationBackgroundService>());
            //NotificationSetting
            services.AddScoped<INotificationSettingRepository, NotificationSettingRepository>();
            services.AddScoped<NotificationSettingService>();
            services.AddAutoMapper(typeof(NotificationSettingProfile));
            //services.AddScoped<JobNotificationBackgroundService>();
            services.AddMemoryCache();
            services.AddSignalR();
            //recommend job
            services.AddScoped<IJobRepository, JobRepository>();
            services.AddScoped<ICVRepository, CVRepository>();
            services.AddScoped<IJobEmbeddingRepository, JobEmbeddingRepository>();
            services.AddScoped<ICVEmbeddingRepository, CVEmbeddingRepository>();
            services.AddScoped<JobRecommendationService>();
            //job-alert
            services.AddScoped<IJobAlertRepository,JobAlertRepository>();
            services.AddScoped<JobAlertService>();
            //cache
            services.AddMemoryCache();
            return services;
        }
    }
}
