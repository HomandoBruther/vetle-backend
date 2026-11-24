using System.ComponentModel.DataAnnotations;

namespace VetleApi.Configuration;

/// <summary>
/// Strongly typed view of the "MongoDb" section in configuration.
/// Bound once at startup via the Options pattern, so the rest of the app
/// never has to read raw configuration strings.
/// [Required] is checked by ValidateDataAnnotations() + ValidateOnStart() in Program.cs,
/// so a missing value crashes the app at startup instead of on the first request.
/// </summary>
public sealed class MongoDbSettings
{
    public const string SectionName = "MongoDb";

    [Required] public string ConnectionString { get; set; } = string.Empty;
    [Required] public string DatabaseName { get; set; } = string.Empty;
}
