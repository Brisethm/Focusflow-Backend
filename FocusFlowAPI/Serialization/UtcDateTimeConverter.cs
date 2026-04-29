using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FocusFlowAPI.Serialization
{
    /// <summary>
    /// Convertidor global que serializa todas las fechas DateTime como UTC con formato ISO 8601 (con Z).
    /// </summary>
    public class UtcDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                throw new JsonException("La fecha no puede ser nula.");

            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException("La fecha debe enviarse como texto ISO 8601.");

            var value = reader.GetString();
            if (string.IsNullOrWhiteSpace(value))
                throw new JsonException("La fecha no puede estar vacía.");

            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed))
                return DateTime.SpecifyKind(parsed, DateTimeKind.Utc);

            throw new JsonException("La fecha debe tener formato ISO 8601, por ejemplo 2026-04-17T21:01 o 2026-04-17T21:01:00Z.");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            var utcValue = value.Kind == DateTimeKind.Utc
                ? value
                : DateTime.SpecifyKind(value, DateTimeKind.Utc);

            writer.WriteStringValue(utcValue.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture));
        }
    }
}