using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PMSS.Application.Interfaces.Services;

namespace PMSS.Infrastructure.Services;

/// <summary>
/// Background service that checks student coding activity weekly (Monday at midnight)
/// and sends notifications to teachers about the least active students in their classes.
/// </summary>
public class StudentActivityMonitorBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<StudentActivityMonitorBackgroundService> _logger;

    public StudentActivityMonitorBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<StudentActivityMonitorBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Student Activity Monitor Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var delay = CalculateDelayUntilNextMonday();
                _logger.LogInformation(
                    "Next student activity check scheduled in {Delay} at {Time}",
                    delay, DateTime.Now.Add(delay));

                await Task.Delay(delay, stoppingToken);

                if (stoppingToken.IsCancellationRequested)
                    break;

                await ExecuteActivityCheckAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Student Activity Monitor Background Service is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Student Activity Monitor Background Service");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        _logger.LogInformation("Student Activity Monitor Background Service stopped");
    }

    private async Task ExecuteActivityCheckAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting scheduled student activity check at {Time}", DateTime.Now);

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var monitorService = scope.ServiceProvider.GetRequiredService<IStudentActivityMonitorService>();

            var result = await monitorService.CheckAndNotifyAllAsync();

            if (result.Success)
            {
                _logger.LogInformation(
                    "Scheduled student activity check completed: {Classes} classes processed, {Notifications} notifications sent",
                    result.Data?.TotalClassesProcessed ?? 0, result.Data?.TotalNotificationsSent ?? 0);
            }
            else
            {
                _logger.LogWarning("Scheduled student activity check completed with issues: {Error}", result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute scheduled student activity check");
        }
    }

    private static TimeSpan CalculateDelayUntilNextMonday()
    {
        var now = DateTime.Now;
        var daysUntilMonday = ((int)DayOfWeek.Monday - (int)now.DayOfWeek + 7) % 7;
        if (daysUntilMonday == 0 && now.TimeOfDay > TimeSpan.Zero)
            daysUntilMonday = 7;

        var nextMonday = now.Date.AddDays(daysUntilMonday);
        var delay = nextMonday - now;

        if (delay < TimeSpan.FromMinutes(1))
            delay = delay.Add(TimeSpan.FromDays(7));

        return delay;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Student Activity Monitor Background Service is stopping...");
        await base.StopAsync(cancellationToken);
    }
}
