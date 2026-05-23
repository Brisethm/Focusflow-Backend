using FocusFlowAPI.Controllers;
using FocusFlowAPI.DTOs;
using FocusFlowAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace FocusFlow.Tests.Controllers
{
    public class AuthControllerTests
    {
        [Fact]
        public async Task Register_DeberiaRetornarOk_CuandoRegistroEsExitoso()
        {
            var registroDto = new RegisterDto
            {
                Email = "test@focusflow.com",
                Password = "Password123!",
                Nombre = "Usuario Test",
                Rol = "user"
            };
            var respuestaExitosa = new AuthResponse
            {
                Success = true,
                Message = "Usuario registrado correctamente",
                UserId = Guid.NewGuid().ToString(),
                Token = "fake-jwt-token",
                RefreshToken = "fake-refresh-token",
                ProfileReady = true
            };
            var mockAuthService = new Mock<IAuthService>();
            mockAuthService.Setup(service => service.RegisterAsync(It.IsAny<RegisterDto>())).ReturnsAsync(respuestaExitosa);
            var controller = new AuthController(mockAuthService.Object);

            var resultado = await controller.Register(registroDto);

            var okResult = Assert.IsType<OkObjectResult>(resultado);
            Assert.Equal(200, okResult.StatusCode);
            mockAuthService.Verify(service => service.RegisterAsync(It.IsAny<RegisterDto>()), Times.Once);
        }

        [Fact]
        public async Task Login_DeberiaRetornarOk_CuandoCredencialesSonValidas()
        {
            var loginDto = new LoginDto { Email = "test@focusflow.com", Password = "Password123!" };
            var authResponse = new AuthResponse
            {
                Success = true,
                Token = "fake-jwt",
                RefreshToken = "fake-refresh",
                UserId = "user-123"
            };
            var mockAuthService = new Mock<IAuthService>();
            mockAuthService.Setup(s => s.LoginAsync(It.IsAny<LoginDto>())).ReturnsAsync(authResponse);
            var controller = new AuthController(mockAuthService.Object);

            var result = await controller.Login(loginDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task Login_DeberiaRetornarError_CuandoServicioFalla()
        {
            var loginDto = new LoginDto { Email = "wrong@test.com", Password = "wrong" };
            var errorResponse = new AuthResponse { Success = false, StatusCode = 401, Message = "Credenciales inválidas" };
            var mockAuthService = new Mock<IAuthService>();
            mockAuthService.Setup(s => s.LoginAsync(It.IsAny<LoginDto>())).ReturnsAsync(errorResponse);
            var controller = new AuthController(mockAuthService.Object);

            var result = await controller.Login(loginDto);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(401, objectResult.StatusCode);
        }

        [Fact]
        public async Task ResetPassword_DeberiaRetornarOk_CuandoEmailEsValido()
        {
            var dto = new ResetPasswordDto { Email = "test@test.com" };
            var mockAuthService = new Mock<IAuthService>();
            mockAuthService.Setup(s => s.ResetPasswordAsync(dto.Email)).ReturnsAsync(true);
            var controller = new AuthController(mockAuthService.Object);

            var result = await controller.ResetPassword(dto);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task ResetPassword_DeberiaRetornarBadRequest_CuandoEmailNoExiste()
        {
            var dto = new ResetPasswordDto { Email = "nonexistent@test.com" };
            var mockAuthService = new Mock<IAuthService>();
            mockAuthService.Setup(s => s.ResetPasswordAsync(dto.Email)).ReturnsAsync(false);
            var controller = new AuthController(mockAuthService.Object);

            var result = await controller.ResetPassword(dto);

            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdatePassword_DeberiaRetornarOk_CuandoTokenYPasswordSonValidos()
        {
            var dto = new UpdatePasswordDto { NewPassword = "NewStrongPassword123!" };
            var mockAuthService = new Mock<IAuthService>();
            mockAuthService.Setup(s => s.UpdatePasswordAsync("valid-token", dto.NewPassword))
                .ReturnsAsync(new AuthResponse { Success = true, Message = "Actualizada" });
            var controller = new AuthController(mockAuthService.Object);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Authorization = "Bearer valid-token";
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = await controller.UpdatePassword("Bearer valid-token", dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task UpdatePassword_DeberiaRetornarBadRequest_CuandoServicioFalla()
        {
            var dto = new UpdatePasswordDto { NewPassword = "Weak" };
            var mockAuthService = new Mock<IAuthService>();
            mockAuthService.Setup(s => s.UpdatePasswordAsync(It.IsAny<string>(), dto.NewPassword))
                .ReturnsAsync(new AuthResponse { Success = false, StatusCode = 400, Message = "Error" });
            var controller = new AuthController(mockAuthService.Object);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Authorization = "Bearer token";
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = await controller.UpdatePassword("Bearer token", dto);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, objectResult.StatusCode);
        }

        [Fact]
        public async Task RegisterStaff_DeberiaRetornarOk_CuandoAdminEstaAutenticado()
        {
            var dto = new RegisterStaffDto
            {
                Email = "staff@focusflow.com",
                Password = "StaffPassword123!",
                Nombre = "Soporte Técnico",
                Rol = "support"
            };
            var adminId = Guid.NewGuid();
            var mockAuthService = new Mock<IAuthService>();
            mockAuthService.Setup(s => s.RegisterStaffAsync(It.IsAny<RegisterStaffDto>(), adminId))
                .ReturnsAsync(new AuthResponse { Success = true, Message = "Staff creado", UserId = Guid.NewGuid().ToString() });
            var controller = new AuthController(mockAuthService.Object);

            var claims = new List<Claim>
            {
                new Claim("sub", adminId.ToString()),
                new Claim(ClaimTypes.NameIdentifier, adminId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = claimsPrincipal };
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = await controller.RegisterStaff(dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            mockAuthService.Verify(s => s.RegisterStaffAsync(It.IsAny<RegisterStaffDto>(), adminId), Times.Once);
        }
    }
}