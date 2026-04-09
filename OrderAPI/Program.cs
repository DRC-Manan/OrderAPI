using Asp.Versioning;
using Asp.Versioning.ApiExplorer; // <- Required namespace for AddVersionedA
using Azure.Core;
using FluentValidation; // Add this using directive
using FluentValidation.AspNetCore; // Add this using directive
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore; // Add this using directive
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using OAPI.Application.Comman;
using OAPI.Application.Commands;
using OAPI.Application.Commands.CreateOrder;
using OAPI.Application.Commands.Login;
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
using System.Buffers.Text;
using System.Diagnostics.Metrics;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Intrinsics.X86;
using System.Threading.RateLimiting;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

	options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
	{
		Name = "Authorization",
		Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
		Scheme = "bearer",
		BearerFormat = "JWT",
		In = Microsoft.OpenApi.Models.ParameterLocation.Header,
		Description = "Enter JWT token like: Bearer {your token}"
	});

	options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
	{
		{
			new Microsoft.OpenApi.Models.OpenApiSecurityScheme
			{
				Reference = new Microsoft.OpenApi.Models.OpenApiReference
				{
					Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
					Id = "Bearer"
				}
			},
			new string[] {}
		}
	});
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

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

// Register command handler as the implementation for the ICommandHandler interface
builder.Services.AddScoped<ICommandHandler<CreateOrderCommand, Result<Guid>>, CreateOrderHandler>();
builder.Services.AddScoped<ICommandHandler<LoginCommand, AuthResponse>, LoginHandler>();
builder.Services.AddScoped<ICommandHandler<RefreshTokenCommand, AuthResponse>, RefreshTokenHandler>();
builder.Services.AddScoped<ICommandHandler<LogoutCommand, Result<string>>, LogoutHandler>();

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

	//// FixedWindowPolicy is the name of the policy that we will apply to our endpoints.
	//options.AddPolicy("FixedWindowPolicy", httpContext =>
	//	RateLimitPartition.GetFixedWindowLimiter(
	//		//// Use the client's IP address as the partition key.
	//		//// If it's not available, use "unknown".
	//		//partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",

	//		// Useful when authentication is added, to limit per user instead of per IP.
	//		------------------------------
	//		//	Your “per user” logic depends on this key:
	//		//	RemoteIpAddress
	//		
	//		//👉 This is not always ideal in production
	//		
	//		//⚠️ Problem with IP - based limiting
	//		//Multiple users behind same NAT → same IP → ❌ unfair throttling
	//		//Mobile users → IP changes → ❌ bypass limit
	//		//✅ Better approach(Production)
	//		
	//		//Use User ID => httpContext.User.Identity?.Name ?? "anonymous"
	//		// or API Key instead => httpContext.Request.Headers["X-Api-Key"]
	//		------------------------------
	//		partitionKey: httpContext.User?.Identity?.Name ?? "anonymous",

	//		factory: partition => new FixedWindowRateLimiterOptions
	//		{
	//			PermitLimit = 10, // Max 10 requests
	//			Window = TimeSpan.FromSeconds(10), // Per 10 seconds
	//			QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
	//			QueueLimit = 0 // No queuing, reject immediately when limit is reached
	//		}));

	options.AddPolicy("strict", httpContext =>
		RateLimitPartition.GetFixedWindowLimiter(
			httpContext.Connection.RemoteIpAddress?.ToString(),
			_ => new FixedWindowRateLimiterOptions
			{
				PermitLimit = 5,
				Window = TimeSpan.FromSeconds(10)
			})
	);

	options.AddPolicy("loose", httpContext =>
		RateLimitPartition.GetFixedWindowLimiter(
			httpContext.Connection.RemoteIpAddress?.ToString(),
			_ => new FixedWindowRateLimiterOptions
			{
				PermitLimit = 100,
				Window = TimeSpan.FromMinutes(1)
			})
	);

	// Token Bucket (Best for Burst Traffic)
	// Meaning:
	//		Bucket size(max capacity) = 20 tokens
	//		Refill rate = 5 tokens every 10 seconds
	//		Queue = 2 requests can wait if no tokens
	options.AddPolicy("token", httpContext =>
	RateLimitPartition.GetTokenBucketLimiter(
		httpContext.Connection.RemoteIpAddress?.ToString(),
		_ => new TokenBucketRateLimiterOptions
		{
			TokenLimit = 20,
			TokensPerPeriod = 5,
			ReplenishmentPeriod = TimeSpan.FromSeconds(10),
			QueueLimit = 2
		}));
});

#endregion

// Remove the Server header for security hardening / Disable Server Header (Important)
builder.WebHost.ConfigureKestrel(options =>
{
	options.AddServerHeader = false;
});


// Configure CORS to allow requests from the frontend application (Important for Real APIs)
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowFrontend", policy =>
	{
		policy.WithOrigins("https://yourfrontend.com")
			  .AllowAnyHeader()
			  .AllowAnyMethod();
	});
});

#region Authentication & Authorization

// Configure Authentication
builder.Services
	.AddAuthentication(options =>
	{
		options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
		options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
	})
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,

			ValidIssuer = builder.Configuration["Jwt:Issuer"],
			ValidAudience = builder.Configuration["Jwt:Audience"],
			IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
			ClockSkew = TimeSpan.Zero // Optional: Reduce default clock skew of 5 minutes
		};
	});

// Enable Authorization

// General authorization
//builder.Services.AddAuthorization();

// Role-based policies authorization
builder.Services.AddAuthorization(options =>
{
	// Define policies based on user roles
	options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
	options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));

	// More granular policies based on claims (if needed)
	options.AddPolicy("CanCreateOrder", policy =>policy.RequireClaim("companyrole", "Admin"));
	options.AddPolicy("CanViewOrder", policy => policy.RequireClaim("companyrole", "Admin", "User"));
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

// Environment-Based Configuration
// In production, we want to enforce security best practices like HTTPS and HSTS.
if (!app.Environment.IsDevelopment())
{
	// Enforce HSTS (HTTP Strict Transport Security)
	app.UseHsts();

	// Enforce HTTPS
	// HTTPS is essential for securing data in transit, protecting
	app.UseHttpsRedirection();
}

// Option 1: Apply Globally (Simple)
app.UseRateLimiter();

app.UseHangfireDashboard(); // UI: /hangfire
app.RegisterRecurringJobs();

// Apply CORS policy
app.UseCors("AllowFrontend");

app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>();

// Authentication should come before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
