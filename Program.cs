using ProceduralFamilyTree;
using static ProceduralFamilyTree.Utilities;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureSwaggerGen(setup =>
{
    setup.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Procedural Family Tree",
        Version = "v1"
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Procedural Family Tree API V1");
    options.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();

app.MapGet("/person", () =>
{
    Person person = new(new Utilities.RandomDateTime(1850, 150).Next(), '?');

    return person;
})
.WithName("GetPerson");

app.MapGet("/person/{year}", (int year) =>
{
    Person person = new(new Utilities.RandomDateTime(year).Next(), '?');

    return person;
})
.WithName("GetPersonByYear");

app.MapGet("/family", (int? generations) =>
{
    var primaryFamily = Family.CreateNewRandomFamily();

    generations ??= 0;
    primaryFamily.CreateGenerations((int)generations);

    primaryFamily.AssignPersonNumbers(primaryFamily.Husband);

    var json = JsonConvert.SerializeObject(primaryFamily, Formatting.Indented,
        new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

    return json;
})
.WithName("GetFamily");

app.MapGet("/family/{year}", (int year) =>
{
    var family = Family.CreateNewRandomFamily(year);

    var json = JsonConvert.SerializeObject(family, Formatting.Indented,
        new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

    return json;
})
.WithName("GetFamilyByYear");

app.Run();