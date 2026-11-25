using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using VetleApi.Configuration;
using VetleApi.Models;

namespace VetleApi.Services;

/// <summary>
/// The only class that knows projects live in MongoDB. Endpoints depend on
/// IProjectService, so the storage can be swapped or faked in tests.
/// </summary>
public interface IProjectService
{
    Task<List<Project>> GetAllAsync(CancellationToken ct = default);
    Task<Project?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<Project> CreateAsync(ProjectRequest request, CancellationToken ct = default);
    Task<bool> UpdateAsync(string id, ProjectRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);
}

public sealed class ProjectService : IProjectService
{
    private readonly IMongoCollection<Project> _projects;

    public ProjectService(IMongoClient client, IOptions<MongoDbSettings> settings)
    {
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _projects = database.GetCollection<Project>("projects");
    }

    public Task<List<Project>> GetAllAsync(CancellationToken ct = default) =>
        _projects.Find(_ => true).SortByDescending(p => p.CreatedAt).ToListAsync(ct);

    public async Task<Project?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        // A string that isn't a valid ObjectId can never match, so skip the database call.
        if (!ObjectId.TryParse(id, out _)) return null;
        return await _projects.Find(p => p.Id == id).FirstOrDefaultAsync(ct);
    }

    public async Task<Project> CreateAsync(ProjectRequest request, CancellationToken ct = default)
    {
        var project = new Project
        {
            Title = request.Title,
            Summary = request.Summary,
            Description = request.Description,
            Period = request.Period,
            Tags = request.Tags ?? [],
            RepositoryUrl = request.RepositoryUrl,
            LiveUrl = request.LiveUrl,
            CreatedAt = DateTime.UtcNow,
        };
        await _projects.InsertOneAsync(project, cancellationToken: ct); // the driver fills in project.Id
        return project;
    }

    public async Task<bool> UpdateAsync(string id, ProjectRequest request, CancellationToken ct = default)
    {
        if (!ObjectId.TryParse(id, out _)) return false;

        var update = Builders<Project>.Update
            .Set(p => p.Title, request.Title)
            .Set(p => p.Summary, request.Summary)
            .Set(p => p.Description, request.Description)
            .Set(p => p.Period, request.Period)
            .Set(p => p.Tags, request.Tags ?? [])
            .Set(p => p.RepositoryUrl, request.RepositoryUrl)
            .Set(p => p.LiveUrl, request.LiveUrl);

        var result = await _projects.UpdateOneAsync(p => p.Id == id, update, cancellationToken: ct);
        return result.MatchedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        if (!ObjectId.TryParse(id, out _)) return false;
        var result = await _projects.DeleteOneAsync(p => p.Id == id, ct);
        return result.DeletedCount > 0;
    }
}
