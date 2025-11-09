using Hangfire;
using IPOPulse.DBContext;
using IPOPulse.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

#region Hangfire Services
builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServer();
#endregion

builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


#region Custom Services
builder.Services.AddScoped<IpoDataService>();
builder.Services.AddSingleton<HangfireJobCleaner>();
#endregion

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHangfireDashboard();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Cleaning the scheduled jobs to prevent limits of API
var cleaner = app.Services.GetRequiredService<HangfireJobCleaner>();
cleaner.ClearAllJobs();

// Schedule recurring jobs after the app (and Hangfire) is fully initialized
var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();
// var ipoDataService = app.Services.GetRequiredService<IpoDataService>();

recurringJobManager.AddOrUpdate<IpoDataService>(
    "FetchIPOData",
    service => service.FetchAndSaveIpoData(),
    "*/5 * * * *");

recurringJobManager.AddOrUpdate<MarketDataService>(
    "FetchIPOData",
    service => service.GetMarketData(),
    "*/5 * * * *");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
