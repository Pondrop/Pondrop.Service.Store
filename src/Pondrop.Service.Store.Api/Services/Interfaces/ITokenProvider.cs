using Pondrop.Service.Store.Api.Models;
using System.Security.Claims;

namespace Pondrop.Service.Store.Api.Services.Interfaces;

public interface ITokenProvider
{
    string AuthenticateShopper(TokenRequest request);

    ClaimsPrincipal ValidateToken(string token);

    string GetClaim(ClaimsPrincipal principal, string claimName);
}

