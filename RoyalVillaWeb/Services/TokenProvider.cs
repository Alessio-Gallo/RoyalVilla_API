using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace RoyalVillaWeb.Services;

public class TokenProvider(IHttpContextAccessor httpContextAccessor) : ITokenProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor; 

    public void ClearToken()
    {
        _httpContextAccessor.HttpContext?.Session.Remove(SD.SessionAccessToken);
        _httpContextAccessor.HttpContext?.Session.Remove(SD.SessionRefreshToken);
    }

    
    public ClaimsPrincipal? CreatePrincipalFromJwtToken(string token)
    {
        if (string.IsNullOrEmpty(token))
            return null;

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            var identity = new ClaimsIdentity(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var emailClaim = jwt.Claims.FirstOrDefault(
                c => c.Type == ClaimTypes.Email
            );

            var nameClaim = jwt.Claims.FirstOrDefault(
                c => c.Type == ClaimTypes.Name
            );

            var roleClaim = jwt.Claims.FirstOrDefault(
                c => c.Type == ClaimTypes.Role
            );

            var idClaim = jwt.Claims.FirstOrDefault(
                c => c.Type == ClaimTypes.NameIdentifier
            );

            if (emailClaim != null)
                identity.AddClaim(new Claim(ClaimTypes.Email, emailClaim.Value));

            if (nameClaim != null)
                identity.AddClaim(new Claim(ClaimTypes.Name, nameClaim.Value));

            if (roleClaim != null)
                identity.AddClaim(new Claim(ClaimTypes.Role, roleClaim.Value));

            if (idClaim != null)
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, idClaim.Value));

            var principal = new ClaimsPrincipal(identity);

            Console.WriteLine("Principal claims:");

            foreach (var claim in principal.Claims)
            {
                Console.WriteLine($"{claim.Type}: {claim.Value}");
            }

            Console.WriteLine($"Is Admin: {principal.IsInRole("Admin")}");

            return principal;
        }
        catch
        {
            return null;
        }
    }

    public string? GetAccessToken()
    {
        return _httpContextAccessor.HttpContext?.Session.GetString(SD.SessionAccessToken);
    }

    public string? GetRefreshToken()
    {
        return _httpContextAccessor.HttpContext?.Session.GetString(SD.SessionRefreshToken);
    }

    public void SetToken(string token, string refreshToken)
    {
        _httpContextAccessor.HttpContext?.Session.SetString(SD.SessionAccessToken, token);
        _httpContextAccessor.HttpContext?.Session.SetString(SD.SessionRefreshToken, refreshToken);
    }
}