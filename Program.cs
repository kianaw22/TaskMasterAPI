using TaskMasterAPI.Data;

using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);


// Add services to the container (Dependency Injection)
builder.Services.AddControllers(); // API Layer

// Configure SQLite with Entity Framework Core
builder.Services.AddDbContext<AppDbContext>(optionsAction: options =>
    options.UseSqlite(connectionString: builder.Configuration.GetConnectionString(name: "DefaultConnection")));

// Build the application
WebApplication app = builder.Build();

// Configure the HTTP request pipeline (middleware)
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Enable HTTPS redirection
app.UseHttpsRedirection();

// Enable routing for API endpoints
app.UseAuthorization();

app.MapControllers(); // Map controller routes

// Create a logger
ILogger<Program> logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("hello");

// Run the application
app.Run();


