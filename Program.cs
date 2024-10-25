using tree_form_API.Models;

var builder = WebApplication.CreateBuilder(args);

// Add CORS policy to allow requests from localhost:3000
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy => policy.WithOrigins("http://localhost:3000")
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

// Configure EmitterDatabaseSettings using the configuration section
builder.Services.Configure<EmitterDatabaseSettings>(
    builder.Configuration.GetSection("EmitterDatabase"));

// Register EmitterService as a singleton
builder.Services.AddSingleton<EmitterService>();

// Add controllers and configure JSON serialization
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Preserve original property names
    });

// Configure Swagger (you may want to restrict this in production)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Enable Swagger UI only in Development environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Optional: Enable Swagger in production with restricted access
    // Uncomment if needed
    // app.UseSwagger();
    // app.UseSwaggerUI(c =>
    // {
    //     c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
    //     c.RoutePrefix = ""; // Set Swagger as root URL if desired
    // });
}

// Apply the CORS policy
app.UseCors("AllowReactApp");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
