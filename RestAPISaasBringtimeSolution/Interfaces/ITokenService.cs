using System.Security.Claims;
using RestAPISaasBringtimeSolution.Models.Entities;

namespace RestAPISaasBringtimeSolution.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(User user, IEnumerable<string> roles, IEnumerable<long> roleIds, long? foundationId, string? foundationSlug);
        ClaimsPrincipal? ValidateToken(string token);
    }
}
