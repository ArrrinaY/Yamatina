namespace InternshipService.Partitioning;

public interface IPartitionService
{
    Task<List<string>> GetExistingPartitionsAsync(CancellationToken cancellationToken = default);
    Task<List<string>> GetRequiredPartitionNamesAsync(DateTime fromDate, int horizonMonths);
    Task<PartitionCheckResult> EnsurePartitionsAsync(CancellationToken cancellationToken = default);
    Task<PartitionCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default);
}
