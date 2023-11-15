using ProceduralFamilyTree;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using Swashbuckle.AspNetCore.Annotations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();
});
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
    options.DocumentTitle = "Procedural Family Tree v1.0";
});

app.UseHttpsRedirection();

app.MapGet("/person", (int? birthYear, int? seed) =>
{
    if (seed != null) {
        Utilities.SetSeed((int)seed);
    }

    Person person = birthYear == null ? new(new Utilities.RandomDateTime(1850, 150).Next(), '?') : new(new Utilities.RandomDateTime((int)birthYear).Next(), '?');

    return person;
})
.WithName("GetPerson");

app.MapGet("/people", (int? birthYear, int? count, int? seed) =>
{
    if (seed != null) {
        Utilities.SetSeed((int)seed);
    }
    count ??= 1;

    var people = new List<Person>();
    for(int x = 0; x < count; x++) {
        Person person = birthYear == null ? new(new Utilities.RandomDateTime(1850, 150).Next(), '?') : new(new Utilities.RandomDateTime((int)birthYear).Next(), '?');
        people.Add(person);
    }

    return people;
})
.WithName("GetPeople");

app.MapGet("/family", (int? marriageYear, int? generations, int? seed) =>
{
    if (seed != null) {
        Utilities.SetSeed((int)seed);
    }
    marriageYear ??= 0;
    Family? primaryFamily = Family.CreateNewRandomFamily((int)marriageYear);

    if(primaryFamily != null) {
        generations ??= 0;
        generations = generations > 5 ? 5 : generations;
        primaryFamily.CreateGenerations((int)generations);

        primaryFamily.AssignPersonNumbers(primaryFamily.Husband);
    }
    var json = JsonConvert.SerializeObject(primaryFamily, Formatting.Indented,
        new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

    return json;
})
.WithName("GetFamily")
.WithMetadata(new SwaggerOperationAttribute(summary: "Generate Family", description: "Generate a nested family JSON object with set number of generations (currently up to 5)."));

app.Run();