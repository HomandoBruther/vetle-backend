namespace VetleApi.Auth;

/// <summary>Authorization policy names, kept as constants so they can't be mistyped.</summary>
public static class AuthPolicies
{
    /// <summary>Only the site owner may create, update or delete content.</summary>
    public const string Owner = "Owner";
}
