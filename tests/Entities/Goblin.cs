namespace MagiQueryTests.Entities;

public class Goblin: EntityBase
{
    public string Name { get; set; } = null!;
    public char FavouriteLetter { get; set; }
    public sbyte IntelligenceLevel { get; set; }
    public short Age { get; set; }
    public ushort PowerLevel { get; set; }
    public byte Stamina { get; set; }
    public uint ExperiencePoints { get; set; }
    public long MagicPower { get; set; }
    public ulong Mana { get; set; }
    public float Strength { get; set; }
    public double Agility { get; set; }
    public decimal Salary { get; set; }
    public bool IsActive { get; set; }
    public Taste Taste { get; set; }
    public DateTime DateOfBirth { get; set; }
    public DateTimeOffset DateOfConception { get; set; }
    public bool? HobbitAncestry { get; set; } = false;
    public Contract? Contract { get; set; }
}