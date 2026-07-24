using System.Security.Claims;

namespace ExpenSR.Helpers
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal principal)
        {
            var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id)
                ? id
                : throw new UnauthorizedAccessException("Token is missing a valid identifier.");
        }

        public static Guid GetCompanyId(this ClaimsPrincipal principal)
        {
            var value = principal.FindFirstValue("companyId");
            return Guid.TryParse(value, out var id)
                ? id
                : throw new UnauthorizedAccessException("Token is missing a valid companyId.");
        }

        public static string GetRole(this ClaimsPrincipal principal)
        {
            return principal.FindFirstValue(ClaimTypes.Role)
                ?? throw new UnauthorizedAccessException("Token is missing a role claim.");
        }
    }
}