using MongoDB.Driver;
using tree_form_API.Models;
using tree_form_API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using tree_form_API.Constants;

var builder = WebApplication.CreateBuilder(args);

// Add CORS policy to allow requests from localhost:3000
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy => policy.WithOrigins("http://localhost:3000")
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

// Configure and validate EmitterDatabaseSettings using the configuration section
builder.Services.Configure<EmitterDatabaseSettings>(
    builder.Configuration.GetSection("EmitterDatabase"));

// Register MongoDB client as a singleton for reuse
builder.Services.AddSingleton<IMongoClient>(s =>
{
    var connectionString = builder.Configuration.GetValue<string>("EmitterDatabase:ConnectionString");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("MongoDB connection string is missing.");
    }
    return new MongoClient(connectionString);
});

// Register IMongoCollection<User> for injection into UserService
builder.Services.AddScoped(sp =>
{
    var settings = sp.GetRequiredService<IOptions<EmitterDatabaseSettings>>().Value;
    var client = sp.GetRequiredService<IMongoClient>();
    var database = client.GetDatabase(settings.DatabaseName);

    // Ensure the collection name for users is specified and retrieve it
    if (!settings.Collections.TryGetValue("Users", out var usersCollectionName) || string.IsNullOrWhiteSpace(usersCollectionName))
    {
        throw new InvalidOperationException("Users collection name is not specified in configuration.");
    }
    
    return database.GetCollection<User>(usersCollectionName);
});

// Register IMongoCollection<Emitter> for injection into EmitterService
builder.Services.AddScoped(sp =>
{
    var settings = sp.GetRequiredService<IOptions<EmitterDatabaseSettings>>().Value;
    var client = sp.GetRequiredService<IMongoClient>();
    var database = client.GetDatabase(settings.DatabaseName);

    if (!settings.Collections.TryGetValue("Emitters", out var emittersCollectionName) || string.IsNullOrWhiteSpace(emittersCollectionName))
    {
        throw new InvalidOperationException("Emitters collection name is not specified in configuration.");
    }
    
    return database.GetCollection<Emitter>(emittersCollectionName);
});

// Register AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Register services as scoped
builder.Services.AddScoped<EmitterService>();
builder.Services.AddScoped<UserService>();

// Configure JWT authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSettings.GetValue<string>("Key");

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException("JWT Key is not provided in configuration.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Set to true in production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole(UserRoles.Admin));
    options.AddPolicy("ReadWritePolicy", policy => policy.RequireRole(UserRoles.Admin, UserRoles.ReadWrite));
});

// Add controllers and configure JSON serialization
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Preserve original property names
    });

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    // Configure JWT authentication for Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\""
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


var app = builder.Build();

// Enable Swagger UI only in Development environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1"));
}

app.UseHttpsRedirection();

// Apply the CORS policy
app.UseCors("AllowReactApp");

app.UseAuthentication(); // Enable authentication middleware
app.UseAuthorization();

app.MapControllers();

app.Run();
