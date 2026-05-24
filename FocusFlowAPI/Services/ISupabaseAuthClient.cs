namespace FocusFlowAPI.Services
{
    public interface ISupabaseAuthClient
    {
        Supabase.Gotrue.Session? CurrentSession { get; }
        Task<Supabase.Gotrue.Session?> SignUpAsync(string email, string password, Supabase.Gotrue.SignUpOptions options);
        Task<Supabase.Gotrue.Session?> SignInAsync(string email, string password);
        Task<bool> ResetPasswordForEmailAsync(string email);
        Task SetSessionAsync(string accessToken, string refreshToken);
        Task<Supabase.Gotrue.User?> UpdateAsync(Supabase.Gotrue.UserAttributes attributes);
    }
}
