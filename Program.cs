using tree_form_API.Models;

var builder = WebApplication.CreateBuilder(args);

// Add CORS policy to allow requests from localhost:3000
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        builder => builder.WithOrigins("http://localhost:3000")
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

// Configure EmitterDatabaseSettings using the configuration section
builder.Services.Configure<EmitterDatabaseSettings>(
    builder.Configuration.GetSection("EmitterDatabase"));

// Register EmitterService as a singleton
builder.Services.AddSingleton<EmitterService>();

// Add AutoMapper by scanning the assembly for profiles
builder.Services.AddAutoMapper(typeof(Program).Assembly);  // Scans the current assembly for AutoMapper profiles

// Add controllers and other services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Enable Swagger UI if in Development environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Apply the CORS policy
app.UseCors("AllowReactApp");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
