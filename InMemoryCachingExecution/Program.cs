using InMemoryCachingExecution.Data;
using InMemoryCachingExecution.Repository;
using InMemoryCachingExecution.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register the InMemoryCachingDbContext with SQL Server connection string
builder.Services.AddDbContext<InMemoryCachingDbContext>(options => 
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register in-memory caching service for caching data in RAM
builder.Services.AddMemoryCache();

//Register LocationRepository as a Scoped Service
builder.Services.AddScoped<LocationRepository>();

builder.Services.AddScoped<CustomLocationRepository>();

// Register the custom CacheManager as a Singleton
// because it should manage keys and cache entries globally within the application.
builder.Services.AddSingleton<CacheManager>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
