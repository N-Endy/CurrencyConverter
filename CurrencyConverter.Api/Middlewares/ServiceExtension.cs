using CurrencyConverter.Application.Logger.Interface;
using CurrencyConverter.Application.Logger.Service;

namespace CurrencyConverter.Api.Middlewares;

public static class ServiceExtension
{
    public static void ConfigureLoggerService(this IServiceCollection services) =>
        services.AddSingleton<ILoggerManager, LoggerManager>();
}
