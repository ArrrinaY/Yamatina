namespace InternshipService.Partitioning;

public class PartitionCheckResult
{
    public bool IsHealthy { get; set; }
    public List<string> ExistingPartitions { get; set; } = [];
    public List<string> MissingPartitions { get; set; } = [];
    public List<string> CreatedPartitions { get; set; } = [];
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
}
