using System.Security.Claims;
using FocusFlowAPI.Extensions;
using Xunit;

namespace FocusFlow.Tests.Extensions
{
    public class ClaimsPrincipalExtensionsTests
    {
        [Fact]
        public void GetAuthenticatedUserId_DeberiaRetornarGuid_CuandoExisteClaimSub()
        {
            var expectedGuid = Guid.NewGuid();
            var claims = new List<Claim> { new Claim("sub", expectedGuid.ToString()) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);

            var result = user.GetAuthenticatedUserId();

            Assert.NotNull(result);
            Assert.Equal(expectedGuid, result);
        }

        [Fact]
        public void GetAuthenticatedUserId_DeberiaRetornarGuid_CuandoExisteClaimIdUsuario()
        {
            var expectedGuid = Guid.NewGuid();
            var claims = new List<Claim> { new Claim("id_usuario", expectedGuid.ToString()) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);

            var result = user.GetAuthenticatedUserId();

            Assert.NotNull(result);
            Assert.Equal(expectedGuid, result);
        }

        [Fact]
        public void GetAuthenticatedUserId_DeberiaRetornarGuid_CuandoExisteClaimNameIdentifier()
        {
            var expectedGuid = Guid.NewGuid();
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, expectedGuid.ToString()) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);

            var result = user.GetAuthenticatedUserId();

            Assert.NotNull(result);
            Assert.Equal(expectedGuid, result);
        }

        [Fact]
        public void GetAuthenticatedUserId_DeberiaRetornarGuid_CuandoExisteIdentityName()
        {
            var expectedGuid = Guid.NewGuid();
            var identity = new ClaimsIdentity("TestAuth", ClaimTypes.Name, ClaimTypes.Role);
            var claimsPrincipal = new ClaimsPrincipal(identity);
            identity.AddClaim(new Claim(ClaimTypes.Name, expectedGuid.ToString()));

            var result = claimsPrincipal.GetAuthenticatedUserId();

            Assert.NotNull(result);
            Assert.Equal(expectedGuid, result);
        }

        [Fact]
        public void GetAuthenticatedUserId_DeberiaRetornarNull_CuandoNingunClaimDeIdExiste()
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.Role, "user") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);

            var result = user.GetAuthenticatedUserId();

            Assert.Null(result);
        }

        [Fact]
        public void GetAuthenticatedUserId_DeberiaRetornarNull_CuandoElIdNoEsUnGuidValido()
        {
            var claims = new List<Claim> { new Claim("sub", "no-soy-un-guid") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);

            var result = user.GetAuthenticatedUserId();

            Assert.Null(result);
        }
    }
}