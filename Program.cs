using ProceduralFamilyTree;
using static ProceduralFamilyTree.Utilities;
using Newtonsoft.Json;

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

app.MapGet("/family", (int? generations = 1) =>
{
    var family = Family.CreateNewRandomFamily();

    for (int x = 0; x < generations; x++)
    {
        foreach (Person child in family.Children)
        {
            Person spouse = new Person(child);
            child.Family = Family.CreateFamily(child, spouse);
            child.Family.CreateChildren();
        }
    }
    family.AssignPersonNumbers(family.Husband);

    var json = JsonConvert.SerializeObject(family, Formatting.Indented,
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