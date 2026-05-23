using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FocusFlowAPI.Serialization
{
    /// <summary>
    /// Convertidor para TimeOnly que serializa como texto en formato HH:mm:ss.
    /// </summary>
    public class TimeOnlyConverter : JsonConverter<TimeOnly?>
    {
        private const string TimeFormat = "HH:mm:ss";

        public override TimeOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException("La hora de descanso debe enviarse como texto en formato HH:mm:ss.");

            var value = reader.GetString();

            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (TimeOnly.TryParseExact(value, TimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var time))
                return time;

            throw new JsonException("La hora de descanso debe tener el formato HH:mm:ss, por ejemplo 22:30:00.");
        }

        public override void Write(Utf8JsonWriter writer, TimeOnly? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteStringValue(value.Value.ToString(TimeFormat, CultureInfo.InvariantCulture));
                return;
            }

            writer.WriteNullValue();
        }
    }
}