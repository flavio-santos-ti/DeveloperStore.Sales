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
    cm.AutoMap(); 
    cm.SetIgnoreExtraElements(true);
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseAuthentication();

// Custom middleware to capture 401 errors.
app.Use(async (context, next) =>
{
    await next.Invoke();

    if (context.Response.StatusCode == 401)
    {
        Console.WriteLine("Unauthorized request: " + context.Request.Path);
    }

    if (context.Request.Headers.ContainsKey("Authorization"))
    {
        var token = context.Request.Headers["Authorization"].ToString();
        Console.WriteLine("Token recebido (depois do next): " + token);
    }
    else
    {
        Console.WriteLine("Token não encontrado no cabeçalho Authorization (depois do next)");
    }
});

app.UseAuthorization();

app.MapControllers();

app.Run();
