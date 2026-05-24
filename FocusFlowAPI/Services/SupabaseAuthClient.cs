namespace FocusFlowAPI.Services
{
    public class SupabaseAuthClient : ISupabaseAuthClient
    {
        private readonly Supabase.Client _client;

        public SupabaseAuthClient(Supabase.Client client)
        {
            _client = client;
        }

        public Supabase.Gotrue.Session? CurrentSession => _client.Auth.CurrentSession;

        public Task<Supabase.Gotrue.Session?> SignUpAsync(string email, string password, Supabase.Gotrue.SignUpOptions options)
        {
            return _client.Auth.SignUp(email, password, options);
        }

        public Task<Supabase.Gotrue.Session?> SignInAsync(string email, string password)
        {
            return _client.Auth.SignIn(email, password);
        }

        public Task<bool> ResetPasswordForEmailAsync(string email)
        {
            return _client.Auth.ResetPasswordForEmail(email);
        }

        public async Task SetSessionAsync(string accessToken, string refreshToken)
        {
            await _client.Auth.SetSession(accessToken, refreshToken, false);
        }

        public Task<Supabase.Gotrue.User?> UpdateAsync(Supabase.Gotrue.UserAttributes attributes)
        {
            return _client.Auth.Update(attributes);
        }
    }
}
