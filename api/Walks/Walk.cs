namespace api.Walks;

public class Walk
{
    public int WalkNumber { get; set; }
    public string IMEI { get; set; } = null!;
    public DateTime WalkDate { get; set; }
    public int PerWalkMinutes { get; set; }
    public int PerDayWalksMinutes { get; set; }
    public double PerWalkTotalDist { get; set; }
    public double PerDayTotalDist { get; set; }
}
