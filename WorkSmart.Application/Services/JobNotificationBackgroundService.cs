using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WorkSmart.Application.Services;
using WorkSmart.Core.Interface;

public class JobNotificationBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<JobNotificationBackgroundService> _logger;
    private readonly ISendMailService _sendMailService;
    private readonly IHostEnvironment _environment;

    public JobNotificationBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<JobNotificationBackgroundService> logger,
        ISendMailService sendMailService,
        IHostEnvironment environment)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _sendMailService = sendMailService;
        _environment = environment;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Job Notification Background Service STARTED at: {time}", DateTimeOffset.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Tính toán thời gian đến 8 giờ sáng hôm sau
                var now = DateTime.Now;
                var scheduledTime = new DateTime(now.Year, now.Month, now.Day, 8, 0, 0);

                if (now > scheduledTime)
                {
                    scheduledTime = scheduledTime.AddDays(1);
                }

                var delay = scheduledTime - now;

                // Trong môi trường dev, sử dụng thời gian ngắn hơn để test
                if (_environment.IsDevelopment())
                {
                    delay = TimeSpan.FromDays(1);  // set thời gian chạy ở đây (sau bao nhiêu phút gửi lại 1 lần )
                }

                _logger.LogInformation("Next job notification check scheduled at: {time}", DateTime.Now + delay);

                await Task.Delay(delay, stoppingToken);

                await ProcessJobNotifications(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in job notification service main loop");
            }
        }
    }

    private async Task ProcessJobNotifications(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Processing job notifications at: {time}", DateTimeOffset.Now);

            using var scope = _scopeFactory.CreateScope();
            var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();
            var notificationService = scope.ServiceProvider.GetRequiredService<NotificationService>();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            // Xử lý job sắp hết hạn (1-3 ngày)
            var expiringJobs = await jobRepository.GetExpiringJobsAsync();
            _logger.LogInformation("Found {Count} expiring jobs", expiringJobs.Count);

            foreach (var job in expiringJobs)
            {
                try
                {
                    var today = DateTime.UtcNow.Date;
                    int daysRemaining = 0;

                    if (job.Deadline.HasValue)
                    {
                        daysRemaining = (job.Deadline.Value.Date - today).Days;
                    }

                    // Gửi thông báo trong ứng dụng
                    await notificationService.CreateJobExpiringNotification(job, job.UserID, daysRemaining);

                    // Gửi email
                    var user = await userRepository.GetById(job.UserID);
                    if (user != null && !string.IsNullOrEmpty(user.Email))
                    {
                        string subject = $"Job #{job.JobID} sắp hết hạn";
                        string body = $@"
                        <html>
                            <body>
                                <h2>Thông báo job sắp hết hạn</h2>
                                <p>Job <strong>{job.Title}</strong> của bạn sẽ hết hạn trong <strong>{daysRemaining} ngày</strong>.</p>
                                <p>Vui lòng xem xét và cập nhật thông tin nếu cần thiết.</p>
                            </body>
                        </html>";

                        await _sendMailService.SendEmailAsync(user.Email, subject, body);
                        _logger.LogInformation("Sent email to {Email} for expiring Job {JobID}", user.Email, job.JobID);
                    }

                    _logger.LogInformation("Created expiring notification for Job {JobID}", job.JobID);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing expiring job {JobID}", job.JobID);
                }
            }

            // Xử lý job đã hết hạn 1 ngày
            var expiredJobs = await jobRepository.GetExpiredJobs();
            _logger.LogInformation("Found {Count} expired jobs", expiredJobs.Count);

            foreach (var job in expiredJobs)
            {
                try
                {
                    // Gửi thông báo trong ứng dụng
                    await notificationService.CreateJobExpiredNotification(job, job.UserID);

                    // Gửi email
                    var user = await userRepository.GetById(job.UserID);
                    if (user != null && !string.IsNullOrEmpty(user.Email))
                    {
                        string subject = $"Job #{job.JobID} đã hết hạn";
                        string body = $@"
                        <html>
                            <body>
                                <h2>Thông báo job đã hết hạn</h2>
                                <p>Job <strong>{job.Title}</strong> của bạn đã hết hạn 1 ngày.</p>
                                <p>Nếu bạn muốn tiếp tục tuyển dụng, vui lòng cập nhật thông tin và gia hạn job.</p>
                            </body>
                        </html>";

                        await _sendMailService.SendEmailAsync(user.Email, subject, body);
                        _logger.LogInformation("Sent email to {Email} for expired Job {JobID}", user.Email, job.JobID);
                    }

                    _logger.LogInformation("Created expired notification for Job {JobID}", job.JobID);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing expired job {JobID}", job.JobID);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ProcessJobNotifications");
        }
    }
}