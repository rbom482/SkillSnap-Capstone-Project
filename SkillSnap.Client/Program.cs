using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SkillSnap.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Configure HttpClient for API calls
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri("https://localhost:5001/") 
});

// Register application services
builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<SkillService>();
builder.Services.AddSingleton<NotificationService>();

await builder.Build().RunAsync();
