using ProceduralFamilyTree;
using Newtonsoft.Json;
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

app.MapGet("/family", (int? marriageYear, int? generations, int? seed, string? surname) =>
{
    seed ??= new Random().Next();
    Utilities.SetSeed((int)seed);
    surname ??= "";
    marriageYear ??= 0;
    Family? primaryFamily = Family.CreateNewRandomFamily((int)marriageYear, surname);

    var op = new Output();

    if(primaryFamily != null) {
        generations ??= 0;
        generations = generations > 10 ? 10 : generations;
        primaryFamily.CreateGenerations((int)generations);

        primaryFamily.AssignPersonNumbers(primaryFamily.Husband);

        Person earliestBirth = primaryFamily.Husband.BirthDate >= primaryFamily.Wife.BirthDate ? primaryFamily.Husband : primaryFamily.Wife;
        Person longestLiving = primaryFamily.Husband.Age >= primaryFamily.Wife.Age ? primaryFamily.Husband : primaryFamily.Wife;
        Person? oldestLiving = null;
        int oldestLivingAge = 0;
        Person latestBirth = primaryFamily.Husband.BirthDate <= primaryFamily.Wife.BirthDate ? primaryFamily.Husband : primaryFamily.Wife;
        var surnames = new Dictionary<string, int> {
            { primaryFamily.Wife.LastName, 1 }
        };
        foreach(Person per in primaryFamily.Husband.GetNestedChildren()) {
            earliestBirth = per.BirthDate > earliestBirth.BirthDate ? earliestBirth : per;
            longestLiving = per.Age > longestLiving.Age ? per : longestLiving;
            if(per.IsAlive() && per.Age > oldestLivingAge) {
                oldestLiving = per;
                oldestLivingAge = per.Age;
            }
            latestBirth = per.BirthDate < latestBirth.BirthDate ? latestBirth : per;
            if (surnames.ContainsKey(per.LastName))
            {
                surnames[per.LastName]++;
            }
            else
            {
                surnames[per.LastName] = 1;
            }
        }
        string mcs = string.Empty;
        int mcsNum = surnames.MaxBy(pair => pair.Value).Value;
        // surnames.TryGetValue(mcs, out int numMCS);
        var mcsItems = surnames.Where(pair => pair.Value == mcsNum).Select(pair => pair.Key).ToList();
        foreach(string mcsName in mcsItems) {
            surnames.TryGetValue(mcsName, out int numMCS);
            mcs += $"{mcsName} ({numMCS}), ";
        }
        mcs = mcs[..^2];

        op = new Output {
            MainFamily = primaryFamily,
            EarliestBirth = earliestBirth.ToString(),
            LongestLiving = longestLiving.ToString(),
            OldestLiving = oldestLiving != null ? oldestLiving.ToString() : "N/A",
            LatestBirth = latestBirth.ToString(),
            MostCommonSurname = mcs,
            TotalPersons = primaryFamily.NumberOfDescendants(primaryFamily.Husband, true),
            Seed = seed
        };
    }

    var json = JsonConvert.SerializeObject(op, Formatting.Indented,
        new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

    return json;
})
.WithName("GetFamily")
.WithMetadata(new SwaggerOperationAttribute(summary: "Generate Family", description: "Generate a nested family JSON object with set number of generations (currently up to 5)."));

app.Run();

class Output {
    public Family MainFamily {get; set;} = null!;

    public string EarliestBirth {get; set;} = string.Empty;
    public string LongestLiving {get;set;} = string.Empty;
    public string OldestLiving {get;set;} = string.Empty;
    public string LatestBirth {get; set;} = string.Empty;
    public string MostCommonSurname {get; set;} = string.Empty;
    public int TotalPersons {get; set;} = 0;
    public int? Seed {get; set;} = 0;
}
