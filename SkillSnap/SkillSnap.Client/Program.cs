using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SkillSnap.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Configure HttpClient for API calls
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri("http://localhost:5194/") // Updated to match the actual API port
});

// Register application services
builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<SkillService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddSingleton<NotificationService>();

// Register state management and performance services
builder.Services.AddScoped<UserSessionService>();
builder.Services.AddScoped<PerformanceMonitorService>();

await builder.Build().RunAsync();
