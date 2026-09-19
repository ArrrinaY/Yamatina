namespace InternshipService.Partitioning;

public interface IPartitionAlertService
{
    Task NotifyMissingPartitionsAsync(PartitionCheckResult result, CancellationToken cancellationToken = default);
    Task NotifyRecoveryAsync(PartitionCheckResult result, CancellationToken cancellationToken = default);
}
