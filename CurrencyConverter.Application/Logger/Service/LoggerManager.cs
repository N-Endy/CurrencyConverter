using CurrencyConverter.Application.Logger.Interface;
using Serilog;

namespace CurrencyConverter.Application.Logger.Service;

public class LoggerManager : ILoggerManager
{
    private readonly ILogger _loggerManager;
    
    public LoggerManager(ILogger loggerManager) => _loggerManager = loggerManager;
    
    public void LogInfo(string message) =>  _loggerManager.Information(message);
    public void LogWarn(string message) => _loggerManager.Warning(message);
    public void LogDebug(string message) => _loggerManager.Debug(message);
    public void LogError(string message) => _loggerManager.Error(message);
}
