namespace WebApiExample.DAL.Entities;

public class Contract: EntityBase
{
    public DateOnly SigningDate {get; set;}
    public ContractDetails Details {get; set;}
}