using Microsoft.IdentityModel.Tokens;

namespace FocusFlowAPI.Security
{
    public sealed class SupabaseJwksCache
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

        private readonly string _jwksUrl;
        private readonly HttpClient _httpClient;
        private readonly SemaphoreSlim _refreshLock = new(1, 1);

        private JsonWebKeySet? _cachedKeySet;
        private DateTimeOffset _expiresAt = DateTimeOffset.MinValue;

        public SupabaseJwksCache(string jwksUrl, HttpClient? httpClient = null)
        {
            _jwksUrl = jwksUrl;
            _httpClient = httpClient ?? new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(5)
            };
        }

        public IEnumerable<SecurityKey> GetSigningKeys(string? kid)
        {
            return GetSigningKeysAsync(kid).GetAwaiter().GetResult();
        }

        private async Task<IEnumerable<SecurityKey>> GetSigningKeysAsync(string? kid)
        {
            if (CanUseCache(kid))
            {
                return _cachedKeySet!.Keys;
            }

            await _refreshLock.WaitAsync();
            try
            {
                if (CanUseCache(kid))
                {
                    return _cachedKeySet!.Keys;
                }

                try
                {
                    var jwksJson = await _httpClient.GetStringAsync(_jwksUrl);
                    var keySet = new JsonWebKeySet(jwksJson);

                    if (keySet.Keys.Count == 0)
                    {
                        throw new SecurityTokenSignatureKeyNotFoundException(
                            "Supabase no devolvió claves de firma en JWKS."
                        );
                    }

                    _cachedKeySet = keySet;
                    _expiresAt = DateTimeOffset.UtcNow.Add(CacheDuration);
                }
                catch (Exception ex)
                {
                    if (_cachedKeySet is not null)
                    {
                        // Si Supabase tarda o falla momentáneamente, seguimos usando la última cache válida.
                    }
                    else
                    {
                        // Si no tenemos cache previa y falla, no podemos continuar.
                        throw new SecurityTokenSignatureKeyNotFoundException(
                            "No fue posible obtener las claves de firma de Supabase.", ex
                        );
                    }
                }

                return _cachedKeySet!.Keys;
            }
            finally
            {
                _refreshLock.Release();
            }
        }

        private bool CanUseCache(string? kid)
        {
            if (_cachedKeySet is null || DateTimeOffset.UtcNow >= _expiresAt)
            {
                return false;
            }

            return string.IsNullOrWhiteSpace(kid) || _cachedKeySet.Keys.Any(key => key.Kid == kid);
        }
    }
}