using System.ComponentModel.DataAnnotations;

namespace MagiQueryTests.Entities;

public abstract class EntityBase
{
    [Key]
    public int Id { get; set; }
        
}
