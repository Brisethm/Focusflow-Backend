namespace FocusFlowAPI.DTOs
{
    public class RegisterDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string Nombre { get; set; }
        public string Rol { get; set; } = "user";
    }

    public class LoginDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }

    public class ResetPasswordDto
    {
        public required string Email { get; set; }
    }

    public class UpdatePasswordDto
    {
        public required string NewPassword { get; set; }
    }
}