using Hangfire;
using Hangfire.SqlServer;
using IPOPulse.DBContext;
using IPOPulse.Services;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

#region Hangfire Services
builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(10),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(10),
        QueuePollInterval = TimeSpan.FromSeconds(15),
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    }));

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 5;
});
#endregion

// EF CORE 
builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    sqlOptions => sqlOptions.CommandTimeout(180))
    );

#region Custom Services
builder.Services.AddSingleton<HangfireJobCleaner>();
builder.Services.AddScoped<IpoDataService>();
builder.Services.AddScoped<MarketDataService>();
builder.Services.AddScoped<IMessageService, MailService>();
builder.Services.AddScoped<AlertService>();
#endregion

builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();

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
app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

// Cleaning the scheduled jobs to prevent limits of API
using (var scope = app.Services.CreateScope())
{
    var cleaner = scope.ServiceProvider.GetRequiredService<HangfireJobCleaner>();
    cleaner.ClearAllJobs();
}

using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    recurringJobManager.AddOrUpdate<IpoDataService>(
        "FetchIPOData",
        service => service.FetchAndSaveIpoData(),
        "00 16 * * 1-5",
        new RecurringJobOptions
        {
            TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata")
        }
    );

    recurringJobManager.AddOrUpdate<MarketDataService>(
        "FetchMarketData",
        service => service.GetMarketData(),
        "30 16 * * 1-5",
        new RecurringJobOptions
        {
            TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata")
        }
    );

    recurringJobManager.AddOrUpdate<AlertService>(
        "TrackBoughtStocks",
        service => service.UpdateCurrentPrice(),
        "00 17 * * 1-5",
        new RecurringJobOptions { 
            TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata") 
        }
    );

    recurringJobManager.AddOrUpdate<AlertService>(
       "SendAlerts",
       service => service.BuyAlert(new IPOPulse.Models.MarketData()
       {
           ISIN = "ISIN",
           Name = "Rubicon Research",
           Symbol = "Rubicon",
           offeredPrice = "485",
           listingDayHigh = "585",
           listingDayLow = "625",
           currentPrice = "655.5",
           counter = 0,
           ID = "44",
           ListingDate = new DateTime(2025-10-16),
       }),
       "*/5 * * * *",
       new RecurringJobOptions
       {
           TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata")
       }
   );
}

#region Static Scheduler

// Schedule recurring jobs after the app (and Hangfire) is fully initialized
//var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();

//recurringJobManager.AddOrUpdate<IpoDataService>(
//    "FetchIPOData",
//    service => service.FetchAndSaveIpoData(),
//    "45 15 * * 1-5",
//    TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata"));

//recurringJobManager.AddOrUpdate<MarketDataService>(
//    "FetchMarketData",
//    service => service.GetMarketData(),
//    "15 16 * * 1-5",
//    TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata")
//);

//recurringJobManager.AddOrUpdate<AlertService>(
//    "TrackBoughtStocks",
//    service => service.UpdateCurrentPrice(),
//    "45 16 * * 1-5",
//    TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata")
//);
#endregion



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
