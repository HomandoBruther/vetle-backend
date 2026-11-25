using System.ComponentModel.DataAnnotations;

namespace VetleApi.Models;

/// <summary>What the API returns. Decoupled from the database model on purpose.</summary>
public sealed record ProjectResponse(
    string Id,
    string Title,
    string? Summary,
    string Description,
    string? Period,
    string? ImageUrl,
    IReadOnlyList<string> Tags,
    string? RepositoryUrl,
    string? LiveUrl,
    DateTime CreatedAt)
{
    public static ProjectResponse FromModel(Project p) =>
        new(p.Id!, p.Title, p.Summary, p.Description, p.Period, p.ImageUrl, p.Tags, p.RepositoryUrl, p.LiveUrl, p.CreatedAt);
}

/// <summary>
/// What clients send to create or replace a project. It has no Id or CreatedAt,
/// so clients cannot set fields the server owns.
/// The attributes are checked automatically by AddValidation() in Program.cs.
/// </summary>
public sealed record ProjectRequest(
    [property: Required, StringLength(100, MinimumLength = 1)] string Title,
    [property: Required, StringLength(5000)] string Description,
    [property: StringLength(300)] string? Summary,
    [property: StringLength(50)] string? Period,
    // Either a site-relative path ("/images/...") or an https URL. [Url] alone would reject
    // relative paths, and allowing any string would let "javascript:..." style values in.
    [property: StringLength(300),
               RegularExpression(@"^(/[^/].*|https://.+)$",
                   ErrorMessage = "ImageUrl must be a path starting with / or an https:// URL.")]
    string? ImageUrl,
    List<string>? Tags,
    [property: Url] string? RepositoryUrl,
    [property: Url] string? LiveUrl);
