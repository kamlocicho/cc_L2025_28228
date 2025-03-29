using Microsoft.EntityFrameworkCore;
using peopleapi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<PeopleDb>(options=>{
    var cs = builder.Configuration.GetConnectionString("Db");
    options.UseSqlServer(cs);
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/people", (PeopleDb db) =>
{
    var people = db.People.ToList();
    return people;
})
.WithName("GetPeople")
.WithOpenApi();

app.MapPost("/people", (PeopleDb db, PersonEntity personEntity) =>
{
    db.People.Add(personEntity);
    db.SaveChanges();
})
.WithName("AddPerson")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
