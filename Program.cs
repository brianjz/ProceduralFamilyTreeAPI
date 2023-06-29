using ProceduralFamilyTree;
using static ProceduralFamilyTree.Utilities;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Procedural Family Tree API V1");
    c.RoutePrefix = string.Empty;
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

app.MapGet("/family", () =>
{
    var family = Family.CreateNewRandomFamily();

    var options = new JsonSerializerOptions();
    options.Converters.Add(new DateOnlyConverter());

    var json = JsonSerializer.Deserialize<object>(JsonSerializer.Serialize(family, options));

    return json;
})
.WithName("GetFamily");

app.MapGet("/family/{year}", (int year) =>
{
    var family = Family.CreateNewRandomFamily(year);

    var options = new JsonSerializerOptions();
    options.Converters.Add(new DateOnlyConverter());

    var json = JsonSerializer.Deserialize<object>(JsonSerializer.Serialize(family, options));

    return json;
})
.WithName("GetFamilyByYear");

app.Run();