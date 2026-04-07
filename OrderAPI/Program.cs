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


app.UseHangfireDashboard(); // UI: /hangfire
app.RegisterRecurringJobs();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
