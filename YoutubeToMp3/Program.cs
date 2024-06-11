using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;

// Create a new web application using the provided arguments
var builder = WebApplication.CreateBuilder(args);

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

// Add services to the container.
builder.Services.AddControllersWithViews(); // Add MVC services to the container
builder.Services.AddSignalR(); // Add SignalR services to the container

// Add configuration to the DI container
builder.Services.AddSingleton(configuration);

var app = builder.Build(); // Build the web application

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // Configure exception handling middleware for non-development environments
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts(); // Add HTTP Strict Transport Security (HSTS) middleware
}

app.UseHttpsRedirection(); // Enable HTTPS redirection
app.UseStaticFiles(); // Configure static file serving

app.UseRouting(); // Enable routing middleware

app.UseAuthorization(); // Enable authorization middleware

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"); // Map default route for MVC controllers

app.Run(); // Execute the application
