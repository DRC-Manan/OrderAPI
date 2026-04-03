using System.Text;

namespace OrderAPI.Middleware
{
	public class RequestResponseLoggingMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

		public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
		{
			_next = next;
			_logger = logger;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			var stopwatch = System.Diagnostics.Stopwatch.StartNew();

			// Read Request Body
			var requestBody = await ReadRequestBody(context.Request);

			// Capture Original Response Body Stream
			var originalBodyStream = context.Response.Body;

			using var responseBody = new MemoryStream();
			context.Response.Body = responseBody;

			try
			{
				await _next(context);

				stopwatch.Stop();

				// Read Response Body
				var responseText = await ReadResponseBody(context.Response);

				// Log everything
				StringBuilder logString = new StringBuilder();

				logString.AppendLine("===== HTTP REQUEST HEADER =====");
				logString.AppendLine($"Headers: {string.Join(", ", context.Request.Headers.Select(h => $"{h.Key}: {h.Value}"))}");
				logString.AppendLine();
				logString.AppendLine("===== HTTP REQUEST =====");
				logString.AppendLine($"Method: {context.Request.Method}");
				logString.AppendLine($"Path: {context.Request.Path}");
				logString.AppendLine($"Query: {context.Request.QueryString}");
				logString.AppendLine($"Request Body: {requestBody}");
				logString.AppendLine();
				logString.AppendLine("===== HTTP RESPONSE =====");
				logString.AppendLine($"Status Code: {context.Response.StatusCode}");
				logString.AppendLine($"Response Body: {responseText}");
				logString.AppendLine();
				logString.AppendLine("===== PERFORMANCE =====");
				logString.AppendLine($"Execution Time: {stopwatch.ElapsedMilliseconds} ms");
				logString.AppendLine();

				_logger.LogInformation(logString.ToString());

				// Copy response back to original stream
				responseBody.Seek(0, SeekOrigin.Begin);
				await responseBody.CopyToAsync(originalBodyStream);
			}
			catch (Exception ex)
			{
				stopwatch.Stop();

				_logger.LogError(ex, "Request failed. Execution Time: {Time} ms",
					stopwatch.ElapsedMilliseconds);

				throw;
			}
		}

		private async Task<string> ReadRequestBody(HttpRequest request)
		{
			request.EnableBuffering();

			using var reader = new StreamReader(
				request.Body,
				encoding: Encoding.UTF8,
				detectEncodingFromByteOrderMarks: false,
				leaveOpen: true);

			var body = await reader.ReadToEndAsync();
			request.Body.Position = 0;

			return body;
		}

		private async Task<string> ReadResponseBody(HttpResponse response)
		{
			response.Body.Seek(0, SeekOrigin.Begin);

			var text = await new StreamReader(response.Body).ReadToEndAsync();

			response.Body.Seek(0, SeekOrigin.Begin);

			return text;
		}
	}
}
