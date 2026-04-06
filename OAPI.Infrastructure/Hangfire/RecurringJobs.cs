using Hangfire;

namespace OAPI.Infrastructure.Hangfire
{
	public static class RecurringJobs
	{
		public static void Register()
		{
			RecurringJob.AddOrUpdate<OutboxProcessor>(
				"ProcessOutboxMessages",
				processor => processor.ProcessAsync(),
				Cron.Minutely);
		}
	}
}
