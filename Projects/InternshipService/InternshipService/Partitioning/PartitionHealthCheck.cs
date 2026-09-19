using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace InternshipService.Partitioning;

public class PartitionHealthCheck(IPartitionService partitionService) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var result = await partitionService.CheckHealthAsync(cancellationToken);

        if (result.IsHealthy)
        {
            return HealthCheckResult.Healthy(
                $"All required partitions exist for table applications ({result.ExistingPartitions.Count} partitions).");
        }

        return HealthCheckResult.Unhealthy(
            $"Missing partitions: {string.Join(", ", result.MissingPartitions)}");
    }
}
