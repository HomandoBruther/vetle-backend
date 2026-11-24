using System.ComponentModel.DataAnnotations;

namespace VetleApi.Configuration;

/// <summary>
/// The "Auth" config section. The key itself must never be committed:
/// store it in user secrets locally and in an environment variable (Auth__ApiKey) in production.
/// </summary>
public sealed class ApiKeySettings
{
    public const string SectionName = "Auth";

    // At least 32 characters so the key cannot realistically be guessed.
    // ValidateOnStart() stops the app from starting with a missing or weak key.
    [Required, MinLength(32)]
    public string ApiKey { get; set; } = string.Empty;
}
