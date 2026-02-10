using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PMSS.Application.Interfaces.Services;

namespace PMSS.Infrastructure.Services;

/// <summary>
/// Background service that automatically synchronizes GitHub contribution data at midnight each day.
/// </summary>
public class GithubDataSyncBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GithubDataSyncBackgroundService> _logger;
    private readonly TimeSpan _syncInterval = TimeSpan.FromHours(24);

    public GithubDataSyncBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<GithubDataSyncBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("GitHub Data Sync Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var delayUntilMidnight = CalculateDelayUntilMidnight();
                _logger.LogInformation("Next GitHub data sync scheduled in {Delay} at {Time}",
                    delayUntilMidnight, DateTime.UtcNow.Add(delayUntilMidnight));

                await Task.Delay(delayUntilMidnight, stoppingToken);

                if (stoppingToken.IsCancellationRequested)
                    break;

                await ExecuteSyncAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("GitHub Data Sync Background Service is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GitHub Data Sync Background Service");
                // Wait for a short period before retrying to avoid tight loop on persistent errors
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        _logger.LogInformation("GitHub Data Sync Background Service stopped");
    }

    private async Task ExecuteSyncAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting scheduled GitHub data synchronization at {Time}", DateTime.UtcNow);

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var syncService = scope.ServiceProvider.GetRequiredService<IGithubDataSyncService>();

            var result = await syncService.SyncAllRepositoriesAsync();

            if (result.Success)
            {
                _logger.LogInformation(
                    "Scheduled GitHub sync completed successfully: {Success}/{Total} repositories synced",
                    result.Data?.SuccessfulSyncs ?? 0, result.Data?.TotalRepositories ?? 0);
            }
            else
            {
                _logger.LogWarning("Scheduled GitHub sync completed with issues: {Error}", result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute scheduled GitHub data synchronization");
        }
    }

    private static TimeSpan CalculateDelayUntilMidnight()
    {
        var now = DateTime.UtcNow;
        var nextMidnight = now.Date.AddDays(1);
        var delay = nextMidnight - now;

        // If we're very close to midnight (within 1 minute), schedule for next day's midnight
        if (delay < TimeSpan.FromMinutes(1))
        {
            delay = delay.Add(TimeSpan.FromDays(1));
        }

        return delay;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("GitHub Data Sync Background Service is stopping...");
        await base.StopAsync(cancellationToken);
    }
}
