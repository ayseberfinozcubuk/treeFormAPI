using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Load MongoDB settings from appsettings.json
var mongoSettings = builder.Configuration.GetSection("MongoDB");
var connectionString = mongoSettings["ConnectionString"];
var databaseName = mongoSettings["DatabaseName"];

// Register MongoDB connection
builder.Services.AddSingleton<IMongoClient, MongoClient>(sp => new MongoClient(connectionString));
builder.Services.AddScoped(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(databaseName));

// Swagger configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register custom services
builder.Services.AddScoped<DataService>();

var app = builder.Build();

// Enable Swagger middleware if in development mode
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"));
}

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
