using Microsoft.EntityFrameworkCore;
using WebDataXplorer.Server.Interfaces;
using WebDataXplorer.Server.Models;
using WebDataXplorer.Server.Repositories;
using WebDataXplorer.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Load variables from appsettings.json
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Add services to the app DI container
builder.Services.AddControllers();
builder.Services.AddOpenApi(); // Enables API documentation https://aka.ms/aspnet/openapi

// Maps repository interface to class and uses httpclientfactory to manage http instance
builder.Services.AddHttpClient<IInventoryRepository, InventoryRepository>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["BrightData:BaseUrl"] ?? string.Empty);
});
builder.Services.AddScoped<InventoryService>(); // Scoped allows one instance per request

//Configures Entity Framework Core with Azure SQL (managed identity)
var connectionString = builder.Configuration["Azure:ConnectionString"];
builder.Services.AddDbContext<SqldbWebDataXplorerContext>(options => options.UseSqlServer(connectionString));

var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapStaticAssets();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("/index.html");
app.Run();

