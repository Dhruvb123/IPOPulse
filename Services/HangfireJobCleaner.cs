using Hangfire;
using Hangfire.Storage;

namespace IPOPulse.Services
{
    public class HangfireJobCleaner
    {
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly JobStorage _jobStorage;

        public HangfireJobCleaner(
            IRecurringJobManager recurringJobManager,
            IBackgroundJobClient backgroundJobClient,
            JobStorage jobStorage)
        {
            _recurringJobManager = recurringJobManager;
            _backgroundJobClient = backgroundJobClient;
            _jobStorage = jobStorage;
        }

        public void DeleteAllRecurringJobs()
        {
            using (var connection = _jobStorage.GetConnection())
            {
                var recurringJobs = connection.GetRecurringJobs();

                foreach (var job in recurringJobs)
                {
                    _recurringJobManager.RemoveIfExists(job.Id);
                }
            }
        }

        public void DeleteAllScheduledAndProcessingJobs()
        {
            var monitor = _jobStorage.GetMonitoringApi();

            var scheduledJobs = monitor.ScheduledJobs(0, int.MaxValue);
            foreach (var job in scheduledJobs)
            {
                _backgroundJobClient.Delete(job.Key);
            }

            var processingJobs = monitor.ProcessingJobs(0, int.MaxValue);
            foreach (var job in processingJobs)
            {
                _backgroundJobClient.Delete(job.Key);
            }
        }

        public void ClearAllJobs()
        {
            DeleteAllRecurringJobs();
            DeleteAllScheduledAndProcessingJobs();
        }
    }
}
