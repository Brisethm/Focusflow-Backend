using System;
using System.Text.Json;
using FocusFlowAPI.Serialization;
using Xunit;

namespace FocusFlowAPI.Tests.Serialization
{
    public class UtcDateTimeConverterTests
    {
        private readonly JsonSerializerOptions _options;

        public UtcDateTimeConverterTests()
        {
            _options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            _options.Converters.Add(new UtcDateTimeConverter());
        }

        private class TestModel
        {
            public DateTime Fecha { get; set; }
        }

        [Fact]
        public void Read_DeberiaRetornarDateTimeUtc_CuandoFormatoConZetaEsValido()
        {
            var json = "{\"fecha\":\"2026-04-17T21:01:00Z\"}";

            var resultado = JsonSerializer.Deserialize<TestModel>(json, _options);

            Assert.NotNull(resultado);
            Assert.Equal(DateTimeKind.Utc, resultado.Fecha.Kind);
            Assert.Equal(new DateTime(2026, 4, 17, 21, 1, 0, DateTimeKind.Utc), resultado.Fecha);
        }

        [Fact]
        public void Read_DeberiaRetornarDateTimeUtc_CuandoFormatoSinZetaEsValido()
        {
            var json = "{\"fecha\":\"2026-04-17T21:01\"}";

            var resultado = JsonSerializer.Deserialize<TestModel>(json, _options);

            Assert.NotNull(resultado);
            Assert.Equal(DateTimeKind.Utc, resultado.Fecha.Kind);
            Assert.Equal(new DateTime(2026, 4, 17, 21, 1, 0, DateTimeKind.Utc), resultado.Fecha);
        }

        [Fact]
        public void Read_DeberiaLanzarJsonException_CuandoCadenaEstaVacia()
        {
            var json = "{\"fecha\":\"   \"}";

            var excepcion = Assert.Throws<JsonException>(() =>
                JsonSerializer.Deserialize<TestModel>(json, _options));

            Assert.Equal("La fecha no puede estar vacía.", excepcion.Message);
        }

        [Fact]
        public void Read_DeberiaLanzarJsonException_CuandoFormatoEsInvalido()
        {
            var json = "{\"fecha\":\"17-04-2026\"}";

            var excepcion = Assert.Throws<JsonException>(() =>
                JsonSerializer.Deserialize<TestModel>(json, _options));

            Assert.Equal("La fecha debe tener formato ISO 8601, por ejemplo 2026-04-17T21:01 o 2026-04-17T21:01:00Z.", excepcion.Message);
        }

        [Fact]
        public void Read_DeberiaLanzarJsonException_CuandoTipoDeTokenNoEsString()
        {
            var json = "{\"fecha\":12345}";

            var excepcion = Assert.Throws<JsonException>(() =>
                JsonSerializer.Deserialize<TestModel>(json, _options));

            Assert.Equal("La fecha debe enviarse como texto ISO 8601.", excepcion.Message);
        }

        [Fact]
        public void Read_DeberiaLanzarJsonException_CuandoValorEsNulo()
        {
            var json = "{\"fecha\":null}";

            var excepcion = Assert.Throws<JsonException>(() =>
                JsonSerializer.Deserialize<TestModel>(json, _options));

            Assert.Equal("La fecha no puede ser nula.", excepcion.Message);
        }

        [Fact]
        public void Write_DeberiaEscribirCadenaIso8601ConZ_CuandoFechaEsUtc()
        {
            var modelo = new TestModel { Fecha = new DateTime(2026, 4, 17, 21, 1, 0, DateTimeKind.Utc) };

            var jsonResultado = JsonSerializer.Serialize(modelo, _options);

            Assert.Contains("\"fecha\":\"2026-04-17T21:01:00Z\"", jsonResultado);
        }

        [Fact]
        public void Write_DeberiaForzarEscribirCadenaIso8601ConZ_CuandoFechaNoEsUtc()
        {
            var modelo = new TestModel { Fecha = new DateTime(2026, 4, 17, 21, 1, 0, DateTimeKind.Unspecified) };

            var jsonResultado = JsonSerializer.Serialize(modelo, _options);

            Assert.Contains("\"fecha\":\"2026-04-17T21:01:00Z\"", jsonResultado);
        }
    }
}