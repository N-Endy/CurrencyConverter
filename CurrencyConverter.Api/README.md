# CurrencyConverterApi

**CurrencyConverterApi** is a RESTful API built with ASP.NET Core (.NET 9.0) that provides real-time and historical currency conversion services. It fetches exchange rates from an external service, stores them in a SQLite database, and exposes endpoints for currency conversion and historical rate retrieval. The project follows a clean architecture with dependency injection, repository pattern, global exception handling, rate-limiting, and background services for periodic data fetching.

## Table of Contents

- [Features](#features)
- [Architecture](#architecture)
- [Prerequisites](#prerequisites)
- [Setup Instructions](#setup-instructions)
- [Running the Application](#running-the-application)
- [API Endpoints](#api-endpoints)
- [Project Structure](#project-structure)

## Features

- **Real-Time Currency Conversion**: Convert amounts between currencies using the latest exchange rates.
- **Historical Rate Retrieval**: Fetch exchange rates for specific date ranges or perform historical conversions.
- **Background Services**: Periodically fetch real-time and historical exchange rates using hosted services (`RealTimeRateFetcher`, `HistoricalRateFetcher`).
- **Repository Pattern**: Abstracts data access with `ICurrencyRepository` for maintainability and testability.
- **Global Exception Handling**: Standardized error responses for validation, database, and external service errors.
- **Rate-Limiting**: Restricts API usage per IP to prevent abuse (e.g., 60 requests per minute for conversion endpoints).
- **API Versioning**: Supports versioned endpoints (e.g., `/api/v1/Currency/*`) using URL segment versioning.
- **Logging**: Uses Serilog for structured logging to console and/or file.
- **SQLite Database**: Stores exchange rates with support for real-time and historical data.

## Architecture

The project follows a **clean architecture** with separation of concerns, organized into four projects:

- **CurrencyConverter.Api**: Contains controllers, middleware, and startup configuration (ASP.NET Core Web API).
- **CurrencyConverter.Application**: Implements business logic, services (`ICurrencyService`, `IExternalCurrencyService`), and validators.
- **CurrencyConverter.Infrastructure**: Handles data access (`ApplicationDbContext`, `ICurrencyRepository`) and database configuration (SQLite).
- **CurrencyConverter.Domain**: Defines entities (`ExchangeRate`) and custom exceptions.

Key technologies and patterns:

- **ASP.NET Core 9.0**: Web framework for building the API.
- **Entity Framework Core**: ORM for SQLite database operations.
- **Repository Pattern**: Abstracts data access with `ICurrencyRepository`.
- **Dependency Injection**: Manages service lifetimes (scoped, singleton).
- **Global Exception Handling**: Middleware for consistent error responses.
- **Rate-Limiting**: Uses `AspNetCoreRateLimit` for IP-based request limits.
- **Background Services**: Hosted services for periodic data fetching.
- **Serilog**: Structured logging.
- **FluentValidation**: Request validation for API inputs.
- **API Versioning**: URL-based versioning with `Asp.Versioning.Mvc`.
- **Polly**: Retry policies for external service calls.

## Prerequisites

To run or develop the project, ensure you have:

- **.NET SDK 9.0**: [Download](https://dotnet.microsoft.com/download/dotnet/9.0)
- **SQLite**: No separate installation needed (uses `Microsoft.Data.Sqlite`).
- **IDE**: Visual Studio 2022, Rider, or VS Code (optional).
- **Git**: For cloning the repository.
- **Postman** or **cURL**: For testing API endpoints.
- **dotnet-ef CLI**: For database migrations:
  ```bash
  dotnet tool install --global dotnet-ef

### Setup Instructions
- Clone the repository:
  ```bash
  git clone https://github.com/your-username/CurrencyConverterApi.git
  cd CurrencyConverterApi

- Restore dependencies:
  ```bash
    dotnet restore

- Apply database migrations:
  ```bash
    cd CurrencyConverter.Api
    dotnet ef migrations add InitialCreate --project ../CurrencyConverter.Infrastructure --startup-project .
    dotnet ef database update --project ../CurrencyConverter.Infrastructure --startup-project .

This creates currencyconverter.db in the CurrencyConverter.Api directory.

### Running the Application
- Run the API:
  ```bash
  cd CurrencyConverter.Api
  dotnet run
  ```
    * The API will be available at `http://localhost:5000` (or `https://localhost:5001` for HTTPS).
    * In development, Swagger UI is available at https://localhost:5001/swagger.


- Verify Startup
    * Check the console for Serilog logs indicating successful startup.
    * Ensure no DI errors or HTTPS warnings appear.


- Test Endpoints:
    * Use Postman or cURL to test the API (see API Endpoints (#api-endpoints)).

## API Endpoints

| Method | Endpoint                                      | Description                                      | Request Body                              | Query Parameters                  | Response Example                                                                 |
|--------|-----------------------------------------------|--------------------------------------------------|-------------------------------------------|-----------------------------------|----------------------------------------------------------------------------------|
| GET    | `/api/v1/Currency/convert`                   | Converts an amount between currencies using the latest rate. | `CurrencyConversionRequest` (JSON)        | None                              | `{"fromCurrency":"USD","toCurrency":"GBP","amount":100,"convertedAmount":80,"rate":0.80,"conversionDate":"2025-04-24T12:00:00Z"}` |
| GET    | `/api/v1/Currency/historical-rates`          | Retrieves historical rates for a date range.     | `CurrencyConversionRequest` (JSON)        | `startDate`, `endDate`            | `[{"conversionDate":"2025-04-01T00:00:00Z","rate":0.79}]`                        |
| GET    | `/api/v1/Currency/convert/historical`        | Converts an amount using a historical rate.      | `CurrencyConversionRequest` (JSON)        | None                              | `{"fromCurrency":"USD","toCurrency":"GBP","amount":100,"convertedAmount":79,"rate":0.79,"conversionDate":"2025-04-01T00:00:00Z"}` |

### Error Responses
- Errors are handled by the GlobalExceptionHandler
  ```json
  {
  "statusCode": 400,
  "errorCode": "VALIDATION_ERROR",
  "message": "Amount must be positive.",
  "timestamp": "2025-04-24T12:00:00Z"
  }
  ```
- Common error codes:
    * 400: Validation errors
    * 404: Not found (e.g., currency not supported)
    * 500: Internal server error (e.g., database issues)
    * 429: Rate limit exceeded
    * 503: Service unavailable (e.g., external service down)

### Rate Limiting
- Rate limits are set to 60 requests per minute per IP for conversion endpoints.
- Exceeding the limit returns a 429 status code with a message:
```json
{
  "statusCode": 429,
  "errorCode": "RATE_LIMIT_EXCEEDED",
  "message": "Rate limit exceeded. Try again later.",
  "timestamp": "2025-04-24T12:00:00Z"
}
```

### Project Structure
```plaintext
  CurrencyConverter/
├── CurrencyConverter.Api/
│   ├── Controllers/
│   │   └──  v1/CurrencyController.cs
│   ├── Middlewares/
│   │   └── GlobalExceptionHandlerMiddleware.cs
│   ├── BackgroundJob/
│   │   ├── HistoricalRateFetcher.cs
│   │   └── RealTimeRateFetcher.cs
│   ├── Program.cs
│   ├── appsettings.json
│   └── CurrencyConverter.Api.csproj
├── CurrencyConverter.Application/
│   ├── CurrencyService/
│   │   ├── Interface/ICurrencyService.cs
│   │   └── Service/CurrencyService.cs
│   ├── ExternalService/
│   │   ├── Interface/IExternalCurrencyService.cs
│   │   └── Service/
│   │       ├── ExchangeRateService.cs
│   │       └── SimulatedCurrencyService.cs
│   ├── Logger/
│   │   └── Interface/ILoggerManager.cs
│   ├── Validators/
│   │   └── CurrencyConversionRequestValidator.cs
│   └── CurrencyConverter.Application.csproj
├── CurrencyConverter.Infrastructure/
│   ├── Database/
│   │   └── ApplicationDbContext.cs
│   ├── Repository/
│   │   ├── Interface/ICurrencyRepository.cs
│   │   └── CurrencyRepository.cs
│   ├── Migrations/
│   └── CurrencyConverter.Infrastructure.csproj
├── CurrencyConverter.Domain/
│   ├── Entities/
│   │   └── ExchangeRate.cs
│   ├── Exceptions/
│   │   └── CustomExceptions.cs
│   ├── Models/
│   │   └── Responses/
│   │       ├── HistoricalResponse.cs
│   │       └── RealTimeResponse.cs
│   └── CurrencyConverter.Domain.csproj
└── README.md


