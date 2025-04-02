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
            var signalRService = scope.ServiceProvider.GetRequiredService<SignalRNotificationService>(); // Lấy từ scope

            var expiringJobs = await jobRepository.GetExpiringJobsAsync();
            _logger.LogInformation("Found {Count} expiring jobs", expiringJobs.Count);

            foreach (var job in expiringJobs)
            {
                try
                {
                    var today = DateTime.UtcNow.Date;
                    int daysRemaining = job.Deadline.HasValue ? (job.Deadline.Value.Date - today).Days : 0;

                    var user = await userRepository.GetById(job.UserID);
                    if (user != null && !string.IsNullOrEmpty(user.Email))
                    {
                        string subject = $"Job #{job.Title} Is About To Expire";
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
                                    .container {{
                                        max-width: 600px;
                                        margin: 0 auto;
                                        padding: 20px;
                                    }}
                                    .header {{
                                        background-color: #0078d4;
                                        color: white;
                                        padding: 20px;
                                        text-align: center;
                                        border-radius: 5px 5px 0 0;
                                    }}
                                    .content {{
                                        padding: 20px;
                                        border: 1px solid #dddddd;
                                        border-top: none;
                                        border-radius: 0 0 5px 5px;
                                    }}
                                    .job-details {{
                                        background-color: #f5f5f5;
                                        padding: 15px;
                                        margin: 15px 0;
                                        border-left: 4px solid #0078d4;
                                    }}
                                    .btn {{
                                        display: inline-block;
                                        background-color: #4CAF50;
                                        color: white;
                                        padding: 12px 30px;
                                        text-decoration: none;
                                        border-radius: 30px;
                                        font-weight: bold;
                                        margin: 20px 0;
                                        text-align: center;
                                    }}
                                    .footer {{
                                        margin-top: 20px;
                                        text-align: center;
                                        color: #777777;
                                        font-size: 12px;
                                    }}
                                    .tags {{
                                        margin-top: 15px;
                                    }}
                                    .tag {{
                                        display: inline-block;
                                        background-color: #f0f8ff;
                                        padding: 5px 10px;
                                        margin-right: 5px;
                                        border-radius: 15px;
                                        font-size: 12px;
                                        color: #4285f4;
                                    }}
                                </style>
                            </head>
                            <body>
                                <div class=""container"">
                                    <div class=""header"">
                                        <h2>Job Expiration Notice</h2>
                                    </div>
                                    <div class=""content"">
                                        <p>Hello,</p>
            
                                        <p>We wanted to let you know that your job posting is about to expire.</p>
            
                                        <div class=""job-details"">
                                            <h3>{job.Title}</h3>
                                            <p><strong>Days Remaining:</strong> {daysRemaining}</p>
                
                                        </div>
            
                                        <p>To extend the posting period or make any changes, please visit your dashboard.</p>
            
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
                        await signalRService.SendNotificationToUser( // Sử dụng từ scope
                           job.UserID,
                           $"Job {job.Title} sắp hết hạn",
                           $"Job sẽ hết hạn trong {daysRemaining} ngày.",
                           $"jobs/detail/{job.JobID}"
                       );
                        _logger.LogInformation("Sent email to {Email} for expiring Job {JobID}", user.Email, job.JobID);
                    }

                    _logger.LogInformation("Created expiring notification for Job {JobID}", job.JobID);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing expiring job {JobID}", job.JobID);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ProcessJobNotifications");
        }
    }

}