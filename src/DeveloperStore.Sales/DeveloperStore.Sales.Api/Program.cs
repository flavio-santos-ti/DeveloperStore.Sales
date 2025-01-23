using DeveloperStore.Sales.Api.Configuration;
using DeveloperStore.Sales.Domain.Models;
using MongoDB.Bson.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDatabaseConfiguration(builder.Configuration);
builder.Services.AddDependencyInjection();
builder.Services.AddJwtAuthentication(builder.Configuration);

BsonClassMap.RegisterClassMap<EventLog>(cm =>
{
    cm.AutoMap(); // Mapeia automaticamente as propriedades da classe
    cm.SetIgnoreExtraElements(true); // Ignorar elementos extras
});


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
