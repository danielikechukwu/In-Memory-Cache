using InMemoryCachingExecution.Data;
using InMemoryCachingExecution.Repository;
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
