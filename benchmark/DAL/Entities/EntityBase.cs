using System.ComponentModel.DataAnnotations;

namespace Benchmark.DAL.Entities;

public abstract class EntityBase
{
    [Key]
    public int Id { get; set; }
        
}
