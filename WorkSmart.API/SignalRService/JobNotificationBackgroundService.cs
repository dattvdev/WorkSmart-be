using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WorkSmart.API.SignalRService;
using WorkSmart.Application.Services;
using WorkSmart.Core.Dto.JobDtos;
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
                var now = DateTime.Now;
                var scheduledTime = new DateTime(now.Year, now.Month, now.Day, 8, 0, 0);

                if (now > scheduledTime)
                {
                    scheduledTime = scheduledTime.AddDays(1);
                }

                var delay = scheduledTime - now;

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
            var jobService = scope.ServiceProvider.GetRequiredService<JobService>(); // Thêm JobService
            var notificationService = scope.ServiceProvider.GetRequiredService<NotificationService>();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var signalRService = scope.ServiceProvider.GetRequiredService<SignalRNotificationService>();

            var expiringJobs = await jobRepository.GetExpiringJobsAsync();
            _logger.LogInformation("Found {Count} expiring jobs", expiringJobs.Count);

            var result = await jobService.HideExpiredJobsAsync();
            _logger.LogInformation("Automatically hid {Count} expired jobs", result.HiddenCount);

            foreach (var expiredJob in result.HiddenJobs)
            {
                try
                {
                    
                    var jobId = expiredJob.JobID;
                    var job = await jobRepository.GetById(jobId);

                    if (job != null)
                    {
                        var user = await userRepository.GetById(job.UserID);
                        if (user != null && !string.IsNullOrEmpty(user.Email))
                        {
                            string subject = $"Job #{job.Title} Has Expired";
                            string body = $@"
                            <html>
                            <head>
                                <style>
                                    body {{
                                        font-family: Arial, sans-serif;
                                        line-height: 1.6;
                                        color: #333333;
                                        margin: 0;
                                        padding: 0;
                                    }}
                                    /* CSS styles khác... */
                                </style>
                            </head>
                            <body>
                                <div class=""container"">
                                    <div class=""header"">
                                        <h2>Job Expiration Notice</h2>
                                    </div>
                                    <div class=""content"">
                                        <p>Hello,</p>
    
                                        <p>We wanted to let you know that your job posting has expired and has been automatically hidden.</p>
    
                                        <div class=""job-details"">
                                            <h3>{job.Title}</h3>
                                            <p><strong>Status:</strong> Hidden (Expired)</p>
                                        </div>
    
                                        <p>If you wish to extend the posting period, please visit your dashboard.</p>
    
                                        <div style=""text-align: center;"">
                                            <a href=""http://localhost:5173/employer/manage-jobs"" class=""btn"">Manage Job Posting</a>
                                        </div>
    
                                        <p>If you have any questions, please don't hesitate to contact us.</p>
    
                                        <p>Best regards,<br>
                                        Recruitment Team</p>
                                    </div>
                                    <div class=""footer"">
                                        <p>© 2025 Tech Corp. All rights reserved.</p>
                                    </div>
                                </div>
                            </body>
                            </html>";

                            await _sendMailService.SendEmailAsync(user.Email, subject, body);

                            await signalRService.SendNotificationToUser(
                                job.UserID,
                                "Job Expired and Hidden",
                                $"Your job posting '{job.Title}' has expired and been hidden.",
                                $"/employer/manage-jobs"
                            );

                            _logger.LogInformation("Sent expiration and hiding notification for Job {JobID}", jobId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending notification for hidden job {JobID}", expiredJob.JobID);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ProcessJobNotifications");
        }
    }
}

