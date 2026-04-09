namespace OrderAPI.Middleware
{
	public class CorrelationIdMiddleware
	{
		private readonly RequestDelegate _next;

		public CorrelationIdMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			if (!context.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
			{
				correlationId = Guid.NewGuid().ToString();
				context.Request.Headers["X-Correlation-ID"] = correlationId;
			}
			context.Response.OnStarting(() =>
			{
				context.Response.Headers["X-Correlation-ID"] = correlationId;
				return Task.CompletedTask;
			});
			//await _next(context);

			using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
			{
				await _next(context);
			}
		}
	}
}
