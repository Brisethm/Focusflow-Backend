using Microsoft.IdentityModel.Tokens;
using FocusFlowAPI.Security;
using Moq;
using Moq.Protected;
using System.Net;
using Xunit;

namespace FocusFlow.Tests.Security
{
    public class SupabaseJwksCacheTests
    {
        private const string FakeJwksUrl = "https://fake-supabase.com/.well-known/jwks.json";
        private const string ValidJwksJson = "{\"keys\":[{\"kty\":\"RSA\",\"use\":\"sig\",\"kid\":\"key-1\",\"alg\":\"RS256\",\"n\":\"u1W_A67S...\",\"e\":\"AQAB\"}]}";
        [Fact]
        public void GetSigningKeys_DeberiaDescargarYRetornarClaves_CuandoElCacheEstaVacio()
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(ValidJwksJson)
                });

            var httpClientMock = new HttpClient(handlerMock.Object);
            var cache = new SupabaseJwksCache(FakeJwksUrl, httpClientMock);

            var keys = cache.GetSigningKeys("key-1");

            Assert.NotEmpty(keys);
            var firstKey = keys.First();
            Assert.Equal("key-1", firstKey.KeyId);

            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
        }
        [Fact]
        public void GetSigningKeys_DeberiaUsarCache_YNoRepetirPeticionHttp()
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(ValidJwksJson)
                });

            var httpClientMock = new HttpClient(handlerMock.Object);
            var cache = new SupabaseJwksCache(FakeJwksUrl, httpClientMock);

            var keysPrimeraVez = cache.GetSigningKeys("key-1");
            var keysSegundaVez = cache.GetSigningKeys("key-1");

            Assert.NotEmpty(keysPrimeraVez);
            Assert.NotEmpty(keysSegundaVez);
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public void GetSigningKeys_DeberiaLanzarExcepcion_CuandoSupabaseDevuelveJwksVacio()
        {
            var jwksVacioJson = "{\"keys\": []}";
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jwksVacioJson)
                });

            var httpClientMock = new HttpClient(handlerMock.Object);
            var cache = new SupabaseJwksCache(FakeJwksUrl, httpClientMock);
            Assert.Throws<SecurityTokenSignatureKeyNotFoundException>(() => cache.GetSigningKeys("key-1"));
        }
    }
}