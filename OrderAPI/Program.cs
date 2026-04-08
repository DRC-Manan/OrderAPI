using Asp.Versioning;
using Asp.Versioning.ApiExplorer; // <- Required namespace for AddVersionedA
using FluentValidation; // Add this using directive
using FluentValidation.AspNetCore; // Add this using directive
using Hangfire;
using Microsoft.EntityFrameworkCore; // Add this using directive
using OAPI.Application.Comman;
using OAPI.Application.Commands;
using OAPI.Application.Commands.CreateOrder;
using OAPI.Application.DTO;
using OAPI.Application.Event;
using OAPI.Application.Queries;
using OAPI.Application.Queries.GetOrder;
using OAPI.Application.Repository;
using OAPI.Application.Services;
using OAPI.Application.Validators;
using OAPI.Infrastructure;
using OAPI.Infrastructure.Hangfire;
using OAPI.Infrastructure.Repository;
using OAPI.Infrastructure.Services;
using OrderAPI;
using OrderAPI.Extensions;
using OrderAPI.Middleware;
using Serilog;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
Log.Logger = new LoggerConfiguration()
	.WriteTo.Console()
	.CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	options.SwaggerDoc("v1", new() { Title = "API V1", Version = "v1" });
	options.SwaggerDoc("v2", new() { Title = "API V2", Version = "v2" });
});
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

builder.Services
	.AddApiVersioning(options =>
	{
		options.AssumeDefaultVersionWhenUnspecified = true;
		options.DefaultApiVersion = new ApiVersion(1, 0);
		options.ReportApiVersions = true;
		options.ApiVersionReader = new UrlSegmentApiVersionReader();
	})
	.AddApiExplorer(options =>
	{
		options.GroupNameFormat = "'v'VVV"; // v1, v1.0
		options.SubstituteApiVersionInUrl = true;
	});

builder.Services.AddMemoryCache();

builder.Services.AddScoped<IOrderRepository, OrderRepository>();

builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("SqlConnection")));
builder.Services.AddHangfire(config =>
	config.UseSqlServerStorage(builder.Configuration.GetConnectionString("SqlConnection")));

builder.Services.AddHangfireServer();

// Register caching service
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();

// Register command handler as the implementation for the ICommandHandler interface
builder.Services.AddScoped<ICommandHandler<CreateOrderCommand, Result<Guid>>, CreateOrderHandler>();

// Register query handler as the implementation for the IQueryHandler interface
builder.Services.AddScoped<IQueryHandler<GetOrdersQuery, PageResult<OrderDto>>, GetOrderQueryHandler>();

// Register email service
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();
builder.Services.AddScoped<OutboxProcessor>();

// Register FluentValidation validators
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderValidator>();

// Register event dispatcher and handlers
builder.Services.AddScoped<IEventDispatcher, EventDispatcher>();
builder.Services.AddScoped<IEventHandler<OrderCreatedEvent>, OrderCreatedEventHandler>();

#region Add Rate Limiting

// Set a global rate limit policy that applies to all endpoints.
builder.Services.AddRateLimiter(options =>
{
	//// When the limit is exceeded, return a 429 Too Many Requests response.
	// options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

	// Custom 429 Response (Important for Real APIs)
	options.OnRejected = async (context, cancellationToken) =>
	{
		context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
		context.HttpContext.Response.ContentType = "application/json";

		var response = new
		{
			Message = "Too many requests. Please try again later.",
		};

		await context.HttpContext.Response.WriteAsJsonAsync(response, cancellationToken);
	};

	// FixedWindowPolicy is the name of the policy that we will apply to our endpoints.
	options.AddPolicy("FixedWindowPolicy", httpContext =>
		RateLimitPartition.GetFixedWindowLimiter(
			// Use the client's IP address as the partition key.
			// If it's not available, use "unknown".
			partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
			factory: partition => new FixedWindowRateLimiterOptions
			{
				PermitLimit = 10, // Max 10 requests
				Window = TimeSpan.FromSeconds(10), // Per 10 seconds
				QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
				QueueLimit = 0 // No queuing, reject immediately when limit is reached
			}));
});

#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI(options =>
	{
		var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

		foreach (var description in provider.ApiVersionDescriptions)
		{
			options.SwaggerEndpoint(
				$"/swagger/{description.GroupName}/swagger.json",
				description.GroupName.ToUpperInvariant());
		}
	});
}

// Option 1: Apply Globally (Simple)
app.UseRateLimiter();

app.UseHangfireDashboard(); // UI: /hangfire
app.RegisterRecurringJobs();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
