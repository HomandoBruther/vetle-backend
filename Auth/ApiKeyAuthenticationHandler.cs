using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using VetleApi.Configuration;

namespace VetleApi.Auth;

/// <summary>
/// An ASP.NET Core authentication scheme that identifies the caller by an X-Api-Key header.
///
/// Authentication answers "who are you?" and is handled here.
/// Authorization answers "are you allowed?" and is handled by RequireAuthorization() on the endpoints.
/// Because this plugs into the standard pipeline, it can later be replaced by JWT
/// login without changing any endpoint code.
/// </summary>
public sealed class ApiKeyAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IOptions<ApiKeySettings> settings)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "ApiKey";
    public const string HeaderName = "X-Api-Key";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // No header means anonymous, not an error. Public GET endpoints keep working.
        if (!Request.Headers.TryGetValue(HeaderName, out var provided) || string.IsNullOrEmpty(provided))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        if (!KeysMatch(provided.ToString(), settings.Value.ApiKey))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid API key."));
        }

        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, "owner")], SchemeName);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    /// <summary>
    /// Constant-time comparison. A normal == stops at the first wrong character, so an
    /// attacker could measure response times to guess the key one character at a time.
    /// Hashing both values first makes them equal length, so the length isn't leaked either.
    /// </summary>
    private static bool KeysMatch(string provided, string expected) =>
        CryptographicOperations.FixedTimeEquals(
            SHA256.HashData(Encoding.UTF8.GetBytes(provided)),
            SHA256.HashData(Encoding.UTF8.GetBytes(expected)));
}
