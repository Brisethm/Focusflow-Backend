using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using FocusFlowAPI.Serialization;
using Xunit;

namespace FocusFlowAPI.Tests.Serialization
{
    public class TimeOnlyConverterTests
    {
        private readonly JsonSerializerOptions _options;

        public TimeOnlyConverterTests()
        {
            _options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            _options.Converters.Add(new TimeOnlyConverter());
        }

        private class TestModel
        {
            public TimeOnly? HoraDescanso { get; set; }
        }

        [Fact]
        public void Read_DeberiaRetornarTimeOnly_CuandoFormatoEsValido()
        {
            var json = "{\"horaDescanso\":\"22:30:00\"}";

            var resultado = JsonSerializer.Deserialize<TestModel>(json, _options);

            Assert.NotNull(resultado);
            Assert.True(resultado.HoraDescanso.HasValue);
            Assert.Equal(new TimeOnly(22, 30, 0), resultado.HoraDescanso.Value);
        }

        [Fact]
        public void Read_DeberiaRetornarNull_CuandoValorEsNull()
        {
            var json = "{\"horaDescanso\":null}";

            var resultado = JsonSerializer.Deserialize<TestModel>(json, _options);

            Assert.NotNull(resultado);
            Assert.False(resultado.HoraDescanso.HasValue);
            Assert.Null(resultado.HoraDescanso);
        }

        [Fact]
        public void Read_DeberiaRetornarNull_CuandoCadenaEstaVacia()
        {
            var json = "{\"horaDescanso\":\"   \"}";

            var resultado = JsonSerializer.Deserialize<TestModel>(json, _options);

            Assert.NotNull(resultado);
            Assert.Null(resultado.HoraDescanso);
        }

        [Fact]
        public void Read_DeberiaLanzarJsonException_CuandoFormatoEsInvalido()
        {
            var json = "{\"horaDescanso\":\"22-30-00\"}";

            var excepcion = Assert.Throws<JsonException>(() =>
                JsonSerializer.Deserialize<TestModel>(json, _options));

            Assert.Equal("La hora de descanso debe tener el formato HH:mm:ss, por ejemplo 22:30:00.", excepcion.Message);
        }

        [Fact]
        public void Read_DeberiaLanzarJsonException_CuandoTipoDeTokenNoEsString()
        {
            var json = "{\"horaDescanso\":12345}";

            var excepcion = Assert.Throws<JsonException>(() =>
                JsonSerializer.Deserialize<TestModel>(json, _options));

            Assert.Equal("La hora de descanso debe enviarse como texto en formato HH:mm:ss.", excepcion.Message);
        }

        [Fact]
        public void Write_DeberiaEscribirCadenaEnFormatoCorrecto_CuandoTieneValor()
        {
            var modelo = new TestModel { HoraDescanso = new TimeOnly(14, 15, 30) };

            var jsonResultado = JsonSerializer.Serialize(modelo, _options);

            Assert.Contains("\"horaDescanso\":\"14:15:30\"", jsonResultado);
        }

        [Fact]
        public void Write_DeberiaEscribirNull_CuandoValorEsNulo()
        {
            var modelo = new TestModel { HoraDescanso = null };

            var jsonResultado = JsonSerializer.Serialize(modelo, _options);

            Assert.Contains("\"horaDescanso\":null", jsonResultado);
        }
    }
}