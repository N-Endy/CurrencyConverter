using Asp.Versioning;
using AspNetCoreRateLimit;
using CurrencyConverter.Api.BackgroundJob;
using CurrencyConverter.Api.Middlewares;
using CurrencyConverter.Application.CurrencyService.Interface;
using CurrencyConverter.Application.CurrencyService.Service;
using CurrencyConverter.Application.ExternalService.Interface;
using CurrencyConverter.Application.ExternalService.Service;
using CurrencyConverter.Application.Validators;
using CurrencyConverter.Infrastructure.Database;
using CurrencyConverter.Infrastructure.Repository.Interface;
using CurrencyConverter.Infrastructure.Repository.Service;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Polly;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

// Add DbContext to container using SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Serilog from appsettings.json
builder.Host.UseSerilog((context, config) => 
    config.ReadFrom.Configuration(context.Configuration));

// Add LogManger to container
builder.Services.ConfigureLoggerService();

// Configure Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

// Exponential Backoff
builder.Services.AddHttpClient("ExternalService")
    .AddTransientHttpErrorPolicy(policy => 
        policy.WaitAndRetryAsync(3, 
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

builder.Services.AddScoped<IExternalCurrencyService, ExchangeRateService>();
builder.Services.AddScoped<SimulatedCurrencyService>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<ICurrencyRepository, CurrencyRepository>();


builder.Services.AddHostedService<RealTimeRateFetcher>();
builder.Services.AddHostedService<HistoricalRateFetcher>();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<CurrencyConversionRequestValidator>();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});


WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseGlobalExceptionHandler();
app.UseHttpsRedirection();
app.UseIpRateLimiting();
app.MapControllers();

await app.RunAsync();
