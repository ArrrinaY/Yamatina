using System.Data.Common;
using Dapper;
using Microsoft.Extensions.Options;

namespace InternshipService.Partitioning;

public class PartitionService(DbConnection connection, IOptions<PartitionSettings> options) : IPartitionService
{
    private readonly DbConnection _connection = connection;
    private readonly PartitionSettings _settings = options.Value;

    public async Task<List<string>> GetExistingPartitionsAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT c.relname
            FROM pg_inherits i
            JOIN pg_class c ON c.oid = i.inhrelid
            JOIN pg_class p ON p.oid = i.inhparent
            WHERE p.relname = @TableName
            ORDER BY c.relname
            """;

        await EnsureConnectionOpenAsync(cancellationToken);
        var rows = await _connection.QueryAsync<string>(
            new CommandDefinition(sql, new { _settings.TableName }, cancellationToken: cancellationToken));
        return rows.ToList();
    }

    public Task<List<string>> GetRequiredPartitionNamesAsync(DateTime fromDate, int horizonMonths)
    {
        var partitions = new List<string>();
        var start = new DateTime(fromDate.Year, fromDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        for (var i = 0; i <= horizonMonths; i++)
        {
            var month = start.AddMonths(i);
            partitions.Add($"{_settings.TableName}_{month:yyyy_MM}");
        }

        return Task.FromResult(partitions);
    }

    public async Task<PartitionCheckResult> EnsurePartitionsAsync(CancellationToken cancellationToken = default)
    {
        var result = await CheckHealthAsync(cancellationToken);

        foreach (var missing in result.MissingPartitions.ToList())
        {
            await CreatePartitionAsync(missing, cancellationToken);
            result.CreatedPartitions.Add(missing);
        }

        if (result.CreatedPartitions.Count > 0)
        {
            result.ExistingPartitions = await GetExistingPartitionsAsync(cancellationToken);
            result.MissingPartitions = result.MissingPartitions.Except(result.CreatedPartitions).ToList();
            result.IsHealthy = result.MissingPartitions.Count == 0;
        }

        return result;
    }

    public async Task<PartitionCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var existing = await GetExistingPartitionsAsync(cancellationToken);
        var required = await GetRequiredPartitionNamesAsync(now, _settings.HorizonMonths);
        var missing = required.Where(name => !existing.Contains(name)).ToList();

        return new PartitionCheckResult
        {
            IsHealthy = missing.Count == 0,
            ExistingPartitions = existing,
            MissingPartitions = missing,
            CheckedAt = now
        };
    }

    private async Task CreatePartitionAsync(string partitionName, CancellationToken cancellationToken)
    {
        var suffix = partitionName[( _settings.TableName.Length + 1)..];
        if (!DateTime.TryParseExact(suffix, "yyyy_MM", null, System.Globalization.DateTimeStyles.AssumeUniversal, out var monthStart))
        {
            throw new InvalidOperationException($"Cannot parse partition name: {partitionName}");
        }

        var fromDate = monthStart.ToString("yyyy-MM-dd");
        var toDate = monthStart.AddMonths(1).ToString("yyyy-MM-dd");

        var sql = $"""
            CREATE TABLE IF NOT EXISTS {partitionName}
            PARTITION OF {_settings.TableName}
            FOR VALUES FROM ('{fromDate}') TO ('{toDate}')
            """;

        await EnsureConnectionOpenAsync(cancellationToken);
        await _connection.ExecuteAsync(new CommandDefinition(sql, cancellationToken: cancellationToken));
    }

    private async Task EnsureConnectionOpenAsync(CancellationToken cancellationToken)
    {
        if (_connection.State != System.Data.ConnectionState.Open)
            await _connection.OpenAsync(cancellationToken);
    }
}
