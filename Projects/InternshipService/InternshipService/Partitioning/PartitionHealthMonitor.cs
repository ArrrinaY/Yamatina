using Microsoft.Extensions.Options;

namespace InternshipService.Partitioning;

public class PartitionHealthMonitor(
    IPartitionService partitionService,
    IPartitionAlertService alertService,
    ILogger<PartitionHealthMonitor> logger,
    IOptions<PartitionSettings> options) : BackgroundService
{
    private readonly PartitionSettings _settings = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("PartitionHealthMonitor started for table {Table}", _settings.TableName);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = await partitionService.CheckHealthAsync(stoppingToken);

                if (result.IsHealthy)
                {
                    await alertService.NotifyRecoveryAsync(result, stoppingToken);
                }
                else
                {
                    logger.LogWarning(
                        "Partition health check CRITICAL. Missing partitions: {Missing}",
                        string.Join(", ", result.MissingPartitions));
                    await alertService.NotifyMissingPartitionsAsync(result, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "PartitionHealthMonitor failed");
            }

            await Task.Delay(TimeSpan.FromMinutes(_settings.HealthCheckIntervalMinutes), stoppingToken);
        }
    }
}
