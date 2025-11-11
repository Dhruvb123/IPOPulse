using Hangfire;
using Hangfire.Storage;

namespace IPOPulse.Services
{
    public class HangfireJobCleaner
    {
        public void DeleteAllRecurringJobs()
        {
            var storage = JobStorage.Current;
            using (var connection = storage.GetConnection())
            {
                var recurringJobs = connection.GetRecurringJobs();

                foreach (var job in recurringJobs)
                {
                    RecurringJob.RemoveIfExists(job.Id);
                }
            }
        }

        public void DeleteAllScheduledAndProcessingJobs()
        {
            var monitor = JobStorage.Current.GetMonitoringApi();

            var scheduledJobs = monitor.ScheduledJobs(0, int.MaxValue);
            foreach (var job in scheduledJobs)
            {
                BackgroundJob.Delete(job.Key);
            }

            var processingJobs = monitor.ProcessingJobs(0, int.MaxValue);
            foreach (var job in processingJobs)
            {
                BackgroundJob.Delete(job.Key);
            }

        }

        public void ClearAllJobs()
        {
            DeleteAllRecurringJobs();
            DeleteAllScheduledAndProcessingJobs();
        }
    }
}
