using Hangfire;
using Hangfire.SqlServer;
using IPOPulse.DBContext;
using IPOPulse.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

#region Hangfire Services
builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    }

    ));

builder.Services.AddHangfireServer();
#endregion

builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


#region Custom Services
builder.Services.AddSingleton<HangfireJobCleaner>();
builder.Services.AddScoped<IpoDataService>();
builder.Services.AddScoped<MarketDataService>();
builder.Services.AddScoped<MessageService>();
builder.Services.AddScoped<AlertService>();
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

recurringJobManager.AddOrUpdate<IpoDataService>(
    "FetchIPOData",
    service => service.FetchAndSaveIpoData(),
    "30 6 * * 1-5",
    TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata"));

recurringJobManager.AddOrUpdate<MarketDataService>(
    "FetchMarketData",
    service => service.GetMarketData(),
    "45 3 * * 1-5",
    TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata")
);

recurringJobManager.AddOrUpdate<AlertService>(
    "TrackBoughtStocks",
    service => service.UpdateCurrentPrice(),
    "15 4 * * 1-5",
    TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata")
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
