using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace VetleApi.Models;

/// <summary>
/// A portfolio project as it is stored in MongoDB (the "projects" collection).
/// This is the persistence model; the API exposes DTOs instead (see ProjectDtos.cs).
/// </summary>
public sealed class Project
{
    // MongoDB stores ids as ObjectId; this lets C# work with a plain string.
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public required string Title { get; set; }
    public required string Description { get; set; }
    public List<string> Tags { get; set; } = [];
    public string? RepositoryUrl { get; set; }
    public string? LiveUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}
