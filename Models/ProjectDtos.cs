using System.ComponentModel.DataAnnotations;

namespace VetleApi.Models;

/// <summary>What the API returns. Decoupled from the database model on purpose.</summary>
public sealed record ProjectResponse(
    string Id,
    string Title,
    string Description,
    IReadOnlyList<string> Tags,
    string? RepositoryUrl,
    string? LiveUrl,
    DateTime CreatedAt)
{
    public static ProjectResponse FromModel(Project p) =>
        new(p.Id!, p.Title, p.Description, p.Tags, p.RepositoryUrl, p.LiveUrl, p.CreatedAt);
}

/// <summary>
/// What clients send to create or replace a project. It has no Id or CreatedAt,
/// so clients cannot set fields the server owns.
/// The attributes are checked automatically by AddValidation() in Program.cs.
/// </summary>
public sealed record ProjectRequest(
    [property: Required, StringLength(100, MinimumLength = 1)] string Title,
    [property: Required, StringLength(2000)] string Description,
    List<string>? Tags,
    [property: Url] string? RepositoryUrl,
    [property: Url] string? LiveUrl);
