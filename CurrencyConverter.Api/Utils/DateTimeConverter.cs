using System.Text.Json;
using System.Text.Json.Serialization;

namespace CurrencyConverter.Api.Utils;

public class DateTimeConverter : JsonConverter<DateTime>
{
    private readonly string _dateFormat;

    public DateTimeConverter(string dateFormat)
    {
        _dateFormat = dateFormat;
    }

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // For deserialization (not needed for this solution, but included for completeness)
        if (reader.GetString() is { } dateString && 
            DateTime.TryParseExact(dateString, _dateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime result))
        {
            return result;
        }
        throw new JsonException($"Invalid date format. Expected format: {_dateFormat}.");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        // Serialize DateTime as dd-MM-yyyy
        writer.WriteStringValue(value.ToString(_dateFormat, System.Globalization.CultureInfo.InvariantCulture));
    }
}
