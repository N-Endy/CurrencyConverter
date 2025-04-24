using Microsoft.AspNetCore.Http;

namespace CurrencyConverter.Domain.Exceptions;

// Base custom exception
public abstract class CustomException : Exception
{
    public int StatusCode { get; }
    public string ErrorCode { get; }

    protected CustomException(string message, int statusCode, string errorCode)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}

public class ValidationException : CustomException
{
    public ValidationException(string message)
        : base(message, StatusCodes.Status400BadRequest, "VALIDATION_ERROR")
    {
    }
}

public class ExternalServiceException : CustomException
{
    public ExternalServiceException(string message)
        : base(message, StatusCodes.Status503ServiceUnavailable, "EXTERNAL_SERVICE_ERROR")
    {
    }
}

public class DatabaseException : CustomException
{
    public DatabaseException(string message)
        : base(message, StatusCodes.Status409Conflict, "DATABASE_ERROR")
    {
    }
}

public class NotFoundException : CustomException
{
    public NotFoundException(string message)
        : base(message, StatusCodes.Status404NotFound, "NOT_FOUND")
    {
    }
}
