using MagiQuery.Extensions;
using MagiQuery.Models;
using WebApiExample.DAL;
using Microsoft.AspNetCore.Mvc;
using WebApiExample.DAL.Entities;

namespace WebApiExample.Controllers;

[Route("Goblins")]
public class QueryController(TestDbContext context) : ControllerBase
{
    [HttpPost("Query")]
    [ProducesResponseType<QueryResponsePaged<Goblin>>(StatusCodes.Status200OK)]
    public IActionResult QueryGoblins([FromBody] QueryRequestPaged request)
        => Ok(context.Goblins.GetPagedResponse(request));
}