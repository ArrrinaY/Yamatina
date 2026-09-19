namespace InternshipService.Partitioning;

public class PartitionSettings
{
    public string TableName { get; set; } = "applications";
    public int HorizonMonths { get; set; } = 3;
    public int JobIntervalMinutes { get; set; } = 60;
    public int HealthCheckIntervalMinutes { get; set; } = 5;
    public string AlertChannel { get; set; } = "log";
}
