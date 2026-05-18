using FocusFlowAPI.DTOs;

namespace FocusFlowAPI.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterDto dto);
        Task<AuthResponse> LoginAsync(LoginDto dto);
        Task<bool> ResetPasswordAsync(string email);
        Task<AuthResponse> UpdatePasswordAsync(string accessToken, string newPassword);
        Task<AuthResponse> RegisterStaffAsync(RegisterStaffDto dto, Guid adminId);
    }
}
