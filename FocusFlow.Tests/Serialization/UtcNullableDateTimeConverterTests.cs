using System;
using System.Text.Json;
using FocusFlowAPI.Serialization;
using Xunit;

namespace FocusFlowAPI.Tests.Serialization
{
    public class UtcNullableDateTimeConverterTests
    {
        private readonly JsonSerializerOptions _options;

        public UtcNullableDateTimeConverterTests()
        {
            _options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            _options.Converters.Add(new UtcNullableDateTimeConverter());
        }

        private class TestModel
        {
            public DateTime? Fecha { get; set; }
        }

        [Fact]
        public void Read_DeberiaRetornarDateTimeUtc_CuandoFormatoConZetaEsValido()
        {
            var json = "{\"fecha\":\"2026-04-17T21:01:00Z\"}";

            var resultado = JsonSerializer.Deserialize<TestModel>(json, _options);

            Assert.NotNull(resultado);
            Assert.True(resultado.Fecha.HasValue);
            Assert.Equal(DateTimeKind.Utc, resultado.Fecha.Value.Kind);
            Assert.Equal(new DateTime(2026, 4, 17, 21, 1, 0, DateTimeKind.Utc), resultado.Fecha);
        }

        [Fact]
        public void Read_DeberiaRetornarDateTimeUtc_CuandoFormatoSinZetaEsValido()
        {
            var json = "{\"fecha\":\"2026-04-17T21:01\"}";

            var resultado = JsonSerializer.Deserialize<TestModel>(json, _options);

            Assert.NotNull(resultado);
            Assert.True(resultado.Fecha.HasValue);
            Assert.Equal(DateTimeKind.Utc, resultado.Fecha.Value.Kind);
            Assert.Equal(new DateTime(2026, 4, 17, 21, 1, 0, DateTimeKind.Utc), resultado.Fecha);
        }

        [Fact]
        public void Read_DeberiaRetornarNull_CuandoValorEsNulo()
        {
            var json = "{\"fecha\":null}";

            var resultado = JsonSerializer.Deserialize<TestModel>(json, _options);

            Assert.NotNull(resultado);
            Assert.False(resultado.Fecha.HasValue);
            Assert.Null(resultado.Fecha);
        }

        [Fact]
        public void Read_DeberiaRetornarNull_CuandoCadenaEstaVacia()
        {
            var json = "{\"fecha\":\"   \"}";

            var resultado = JsonSerializer.Deserialize<TestModel>(json, _options);

            Assert.NotNull(resultado);
            Assert.False(resultado.Fecha.HasValue);
            Assert.Null(resultado.Fecha);
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
        public void Write_DeberiaEscribirCadenaIso8601ConZ_CuandoFechaTieneValorYEsUtc()
        {
            var modelo = new TestModel { Fecha = new DateTime(2026, 4, 17, 21, 1, 0, DateTimeKind.Utc) };

            var jsonResultado = JsonSerializer.Serialize(modelo, _options);

            Assert.Contains("\"fecha\":\"2026-04-17T21:01:00Z\"", jsonResultado);
        }

        [Fact]
        public void Write_DeberiaForzarEscribirCadenaIso8601ConZ_CuandoFechaTieneValorYNoEsUtc()
        {
            var modelo = new TestModel { Fecha = new DateTime(2026, 4, 17, 21, 1, 0, DateTimeKind.Unspecified) };

            var jsonResultado = JsonSerializer.Serialize(modelo, _options);

            Assert.Contains("\"fecha\":\"2026-04-17T21:01:00Z\"", jsonResultado);
        }

        [Fact]
        public void Write_DeberiaEscribirNull_CuandoValorEsNulo()
        {
            var modelo = new TestModel { Fecha = null };

            var jsonResultado = JsonSerializer.Serialize(modelo, _options);

            Assert.Contains("\"fecha\":null", jsonResultado);
        }
    }
}