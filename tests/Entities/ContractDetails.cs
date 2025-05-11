namespace MagiQueryTests.Entities;

public class ContractDetails: EntityBase
{
    public TimeOnly SigningTime {get; set;}
    public TimeSpan? Duration {get; set;}
    public int? DaysOfEffect {get; set;}
}