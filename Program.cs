using TaskMasterAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TaskMasterAPI.services;
using TaskMasterAPI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);


// Configure SQLite with Entity Framework Core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add JWT Authentication
var loggerFactory = builder.Services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();
var logger = loggerFactory.CreateLogger<Program>();

builder.Services.AddHttpClient();

builder.Services.AddAuthentication(options =>
    {
         options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        //options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;

    }).AddJwtBearer(options =>
    {
        options.UseSecurityTokenValidators = true;
        options.IncludeErrorDetails= true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
             ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
           
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
                logger.LogError("Authentication failed: {ExceptionMessage}", context.Exception.Message);

                if (context.Exception is SecurityTokenExpiredException)
                {
                    logger.LogError("Token is expired.");
                }
                else if (context.Exception is SecurityTokenInvalidSignatureException)
                {
                    logger.LogError("Invalid token signature.");
                }
                else
                {
                    logger.LogError("Token validation failed for another reason.");
                }

                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Token validated for user: {User}", context.Principal.Identity.Name);
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
                logger.LogWarning("JWT Bearer challenge triggered: {Error}, {ErrorDescription}", context.Error, context.ErrorDescription);
                return Task.CompletedTask;
            }
        };
    });
    builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "TaskMaster API",
        Description = "API for managing tasks and GitHub issue integration",
        Contact = new OpenApiContact
        {
            Name = "Your Name",
            Email = "your-email@example.com"
        }
    });

    // Configure JWT Bearer authentication for Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter your JWT with Bearer into field (e.g., 'Bearer {token}')",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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

//builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

// Add services to the container (Dependency Injection)
builder.Services.AddControllers(); // API Layer
// Add HttpContextAccessor for accessing claims in services
builder.Services.AddHttpContextAccessor();

// Register custom services (UserService, TaskService, etc.)
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IGitHubIssueService, GitHubIssueService>();
builder.Services.AddScoped<ITaskService, TaskService>();

// Build the application
var app = builder.Build();
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskMaster API v1");
        c.RoutePrefix = string.Empty;  // Set Swagger UI to be the root URL of the app
    });
}


app.UseHttpsRedirection();

// Enable Authentication and Authorization middleware
app.UseAuthentication();  // Use JWT Authentication middleware
app.UseAuthorization();   // Use Authorization middleware

// Map Controllers
app.MapControllers(); // Map controller routes

;

// Run the application
app.Run();
