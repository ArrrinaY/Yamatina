using Microsoft.Extensions.Options;

namespace InternshipService.Partitioning;

public class PartitionAlertService(
    ILogger<PartitionAlertService> logger,
    IOptions<PartitionSettings> options) : IPartitionAlertService
{
    private readonly PartitionSettings _settings = options.Value;
    private bool _isInAlertState;

    public Task NotifyMissingPartitionsAsync(PartitionCheckResult result, CancellationToken cancellationToken = default)
    {
        if (_isInAlertState)
        {
            logger.LogWarning(
                "Partition alert suppressed (already sent). Table: {Table}, missing: {Missing}",
                _settings.TableName,
                string.Join(", ", result.MissingPartitions));
            return Task.CompletedTask;
        }

        _isInAlertState = true;

        var message = $"""
            🚨 Partition alert
            Table: {_settings.TableName}
            Missing partitions:
            {string.Join(Environment.NewLine, result.MissingPartitions)}
            Expected horizon: {_settings.HorizonMonths} months
            Checked at: {result.CheckedAt:yyyy-MM-dd HH:mm:ss} UTC
            Channel: {_settings.AlertChannel}
            """;

        logger.LogCritical(message);
        return Task.CompletedTask;
    }

    public Task NotifyRecoveryAsync(PartitionCheckResult result, CancellationToken cancellationToken = default)
    {
        if (!_isInAlertState)
        {
            return Task.CompletedTask;
        }

        _isInAlertState = false;

        var message = $"""
            🟢 Partition check OK
            Table: {_settings.TableName}
            All required partitions exist.
            Checked at: {result.CheckedAt:yyyy-MM-dd HH:mm:ss} UTC
            """;

        logger.LogInformation(message);
        return Task.CompletedTask;
    }
}
