using System.Text.Json.Serialization;
using WebApiExample.DAL;
using WebApiExample.Filters;
using Microsoft.AspNetCore.Mvc;

[assembly: ApiController]
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddDbContext<TestDbContext>(ServiceLifetime.Singleton)
    .AddSwaggerGen(x=>x.OperationFilter<QueryRequestExamplesFilter>())
    .AddControllers()
    .AddJsonOptions(x => x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

app.UseSwagger()
    .UseSwaggerUI()
    .UseHttpsRedirection();

app.MapControllers();

app.Services.GetRequiredService<TestDbContext>().Seed();

await app.RunAsync();