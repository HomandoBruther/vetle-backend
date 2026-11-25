using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace VetleApi.Models;

/// <summary>
/// A portfolio project as it is stored in MongoDB (the "projects" collection).
/// This is the persistence model; the API exposes DTOs instead (see ProjectDtos.cs).
/// </summary>
/// <remarks>
/// [BsonIgnoreExtraElements]: by default the driver throws if a document has a field this class
/// doesn't know. That breaks older versions of the app (e.g. during a deploy, or a forgotten
/// running instance) as soon as a newer version writes a new field. Ignoring unknown fields
/// lets old code keep reading newer documents.
/// </remarks>
[BsonIgnoreExtraElements]
public sealed class Project
{
    // MongoDB stores ids as ObjectId; this lets C# work with a plain string.
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public required string Title { get; set; }

    /// <summary>One or two sentences for the project card. Optional: documents created before this field existed have none.</summary>
    public string? Summary { get; set; }

    public required string Description { get; set; }

    /// <summary>When the project happened, as free text, e.g. "2025" or "2024 – present". Not the same as CreatedAt.</summary>
    public string? Period { get; set; }
    public List<string> Tags { get; set; } = [];
    public string? RepositoryUrl { get; set; }
    public string? LiveUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}
