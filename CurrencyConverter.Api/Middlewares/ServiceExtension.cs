using CurrencyConverter.Application.Logger.Interface;
using CurrencyConverter.Application.Logger.Service;

namespace CurrencyConverter.Api.Middlewares;

public static class ServiceExtension
{
    public static IServiceCollection ConfigureLoggerService(this IServiceCollection services)
    {
        services.AddSingleton<ILoggerManager, LoggerManager>();
        return services;
    }

}
