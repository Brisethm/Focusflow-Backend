using System.Security.Claims;

namespace FocusFlowAPI.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid? GetAuthenticatedUserId(this ClaimsPrincipal user)
        {
            var userId = user.FindFirst("sub")?.Value
                ?? user.FindFirst("id_usuario")?.Value
                ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user.Identity?.Name;

            return Guid.TryParse(userId, out var parsedUserId) ? parsedUserId : null;
        }
    }
}
