using System.Text.Json.Serialization;
using MagiQuery.Extensions;
using MagiQuery.Models;
using WebApiExample.DAL;
using WebApiExample.Filters;
using Microsoft.AspNetCore.Mvc;
using WebApiExample.DAL.Entities;

[assembly: ApiController]
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddDbContext<TestDbContext>(ServiceLifetime.Singleton)
    .AddSwaggerGen(x => x.OperationFilter<QueryRequestExamplesFilter>())
    .AddOpenApi()
    .ConfigureHttpJsonOptions(x => x.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

app.UseSwagger()
    .UseSwaggerUI()
    .UseHttpsRedirection();

app.MapOpenApi();

app
    .MapPost(
        "Goblins/Query",
        async (QueryRequestPaged request, TestDbContext context)
            => Results.Ok(await context.Goblins.GetPagedResponseAsync(request)))
    .Produces<QueryResponsePaged<Goblin>>();

app.Services.GetRequiredService<TestDbContext>().Seed();

await app.RunAsync();