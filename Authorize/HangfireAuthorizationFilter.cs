using Hangfire.Dashboard;

namespace IPOPulse.Authorize
{
    public class HangfireAuthorizationFilter: IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var http = context.GetHttpContext();
            var isLoggedIn = http.Session.GetString("IsLoggedIn");

            return isLoggedIn == "true";
        }
    }
}
