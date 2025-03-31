using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

public class BackgroundJobService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Console.WriteLine($"Công việc chạy nền lúc: {DateTime.Now}");
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Chạy mỗi 1 phút
        }
    }
}
