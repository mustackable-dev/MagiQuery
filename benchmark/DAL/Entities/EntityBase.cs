using System.ComponentModel.DataAnnotations;

namespace WebApiExample.DAL.Entities;

public abstract class EntityBase
{
    [Key]
    public int Id { get; set; }
        
}
