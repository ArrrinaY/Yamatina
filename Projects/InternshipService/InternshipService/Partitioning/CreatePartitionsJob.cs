using Microsoft.Extensions.Options;

namespace InternshipService.Partitioning;

public class CreatePartitionsJob(
    IPartitionService partitionService,
    ILogger<CreatePartitionsJob> logger,
    IOptions<PartitionSettings> options) : BackgroundService
{
    private readonly PartitionSettings _settings = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("CreatePartitionsJob started for table {Table}", _settings.TableName);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = await partitionService.EnsurePartitionsAsync(stoppingToken);

                logger.LogInformation(
                    "Partition job finished. Existing: {Existing}, Required horizon: {Horizon}, Missing: {Missing}, Created: {Created}",
                    result.ExistingPartitions.Count,
                    _settings.HorizonMonths,
                    result.MissingPartitions.Count,
                    result.CreatedPartitions.Count);

                foreach (var created in result.CreatedPartitions)
                {
                    logger.LogInformation("Partition created successfully: {Partition}", created);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "CreatePartitionsJob failed");
            }

            await Task.Delay(TimeSpan.FromMinutes(_settings.JobIntervalMinutes), stoppingToken);
        }
    }
}
