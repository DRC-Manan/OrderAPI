using Hangfire;
using OAPI.Infrastructure.Hangfire;

namespace OrderAPI.Extensions
{
	public static class HangfireJobsExtensions
	{
		public static void RegisterRecurringJobs(this IApplicationBuilder app)
		{
			RecurringJob.AddOrUpdate<OutboxProcessor>(
				"process-outbox",
				x => x.ProcessAsync(),
				Cron.Minutely);
		}
	}
}
