using Lms.Api.Application.Interfaces;
using Lms.Api.Application.Services;
using Lms.Api.Common;
using Lms.Api.Infrastructure.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using Serilog;

// Configure Serilog from appsettings.json with configurable log path
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .Build())
    .CreateLogger();

try
{
  Log.Information("Starting LMS Dashboard API application");

var builder = WebApplication.CreateBuilder(args);

// Use Serilog for logging
builder.Host.UseSerilog();

// Controllers + Swagger/OpenAPI
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddApiVersioning(o =>
{
  o.AssumeDefaultVersionWhenUnspecified = true;
  o.DefaultApiVersion = new ApiVersion(1, 0);
  o.ReportApiVersions = true;
});

builder.Services.AddSwaggerGen(opt =>
{
  opt.SwaggerDoc(Constants.Swagger.Version, new OpenApiInfo
  {
    Title = Constants.Swagger.Title,
    Version = Constants.Swagger.Version,
    Description = Constants.Swagger.Description
  });

  // XML docs – make sure GenerateDocumentationFile is true (we set it globally)
  var xmlFile = $"{typeof(Program).Assembly.GetName().Name}.xml";
  var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
  if (File.Exists(xmlPath))
  {
    opt.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
  }
});

// Perf & resiliency: response compression, in-memory cache for quick lookups,
// output cache for GET endpoints, health checks, and basic fixed-window rate limiting.
builder.Services.AddResponseCompression();
builder.Services.AddMemoryCache();
builder.Services.AddOutputCache(o =>
{
  o.AddPolicy(Constants.CachePolicies.ShortList, p => p.Expire(Constants.CacheDurations.ShortList));
});

builder.Services.AddHealthChecks();
builder.Services.AddRateLimiter(o =>
{
  o.AddFixedWindowLimiter("fixed", opt =>
  {
    opt.Window = TimeSpan.FromSeconds(1);
    opt.PermitLimit = 100; // demo: 100 RPS
    opt.QueueLimit = 50;
    opt.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
  });
});

// CORS for local dev (Vite default port 5173, also allow 3000 and 3001)
builder.Services.AddCors(opt =>
{
  // What i would do normally for example
  opt.AddPolicy("Dev", p =>
      p.WithOrigins("http://localhost:5173", "http://localhost:3000", "http://localhost:3001")
       .AllowAnyHeader()
       .AllowAnyMethod());

  // For assigment purposes 
  opt.AddPolicy("Local", p =>
    p.AllowAnyOrigin()
    .AllowAnyHeader()
    .AllowAnyMethod());
});

// DI: services behind interfaces (implementations are internal sealed; contracts are public)
builder.Services.AddSingleton<ICourseService, CourseService>();
builder.Services.AddSingleton<IStudentService, StudentService>();
builder.Services.AddSingleton<IEnrollmentService, EnrollmentService>();
builder.Services.AddSingleton<IReportService, ReportService>();

// DI: AWS S3 mock service (simulates AWS S3 without actual AWS SDK)
builder.Services.AddSingleton<IS3Service, MockS3Service>();

// DI: Data seeding service (only used in Development)
builder.Services.AddScoped<Lms.Api.Infrastructure.Data.DataSeeder>();

// DI: ExceptionHandlingMiddleware implements IMiddleware, so it MUST be registered
builder.Services.AddTransient<ExceptionHandlingMiddleware>();
// Note: IdempotencyMiddleware uses convention-based approach, so it's NOT registered in DI

// Kestrel timeouts
builder.WebHost.ConfigureKestrel(k =>
{
  k.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(120);
  k.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(15);
});

var app = builder.Build();

Log.Information("Application starting in {Environment} environment", app.Environment.EnvironmentName);

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
  Log.Information("Swagger UI is available at /swagger");
}

app.UseResponseCompression();

// CORS Configuration - Must be before authentication/authorization
Log.Information("Enabling CORS for origins: http://localhost:5173, http://localhost:3000, http://localhost:3001");
app.UseCors("Local");

app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseOutputCache();

app.MapHealthChecks("/healthz");

// Idempotency for POST requests (must be before exception handling)
app.UseMiddleware<IdempotencyMiddleware>();

// Global exception → RFC7807 ProblemDetails
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();
Log.Information("Controllers mapped successfully");

// DATA SEEDING
// Seed initial data if enabled in configuration (appsettings.Development.json)
// To enable/disable: Set "DataSeeding:Enabled" to true/false in appsettings
if (app.Environment.IsDevelopment())
{
  var seedingEnabled = app.Configuration.GetValue<bool>("DataSeeding:Enabled");
  if (seedingEnabled)
  {
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<Lms.Api.Infrastructure.Data.DataSeeder>();
    await seeder.SeedAsync();
  }
}

Log.Information("LMS Dashboard API is ready and listening on http://localhost:5225");
if (app.Environment.IsDevelopment())
{
  Log.Information("Swagger documentation available at http://localhost:5225/swagger");
}

app.Run();
}
catch (Exception ex)
{
  Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
  Log.Information("Application shutting down");
  Log.CloseAndFlush();
}
