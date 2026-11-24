using Microsoft.AspNetCore.Http.HttpResults;
using VetleApi.Auth;
using VetleApi.Models;
using VetleApi.Services;

namespace VetleApi.Endpoints;

/// <summary>
/// All /api/projects routes in one place. Program.cs only calls MapProjectEndpoints(),
/// so it stays short as more resources are added.
/// </summary>
public static class ProjectEndpoints
{
    public static RouteGroupBuilder MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects").WithTags("Projects");

        // Public: anyone can read projects.
        group.MapGet("/", GetAll).WithName("GetProjects");
        group.MapGet("/{id}", GetById).WithName("GetProjectById");

        // Protected: a nested group with the same prefix, where every route requires the Owner policy.
        // Putting the requirement on the group means a new write endpoint can't be left unprotected by accident.
        var owner = group.MapGroup("").RequireAuthorization(AuthPolicies.Owner);
        owner.MapPost("/", Create).WithName("CreateProject");
        owner.MapPut("/{id}", Update).WithName("UpdateProject");
        owner.MapDelete("/{id}", Delete).WithName("DeleteProject");

        return group;
    }

    // Typed results such as Ok<T> and NotFound tell OpenAPI exactly which
    // responses each endpoint can return, and the compiler checks them.

    private static async Task<Ok<List<ProjectResponse>>> GetAll(IProjectService service, CancellationToken ct)
    {
        var projects = await service.GetAllAsync(ct);
        return TypedResults.Ok(projects.Select(ProjectResponse.FromModel).ToList());
    }

    private static async Task<Results<Ok<ProjectResponse>, NotFound>> GetById(
        string id, IProjectService service, CancellationToken ct)
    {
        var project = await service.GetByIdAsync(id, ct);
        return project is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(ProjectResponse.FromModel(project));
    }

    private static async Task<Created<ProjectResponse>> Create(
        ProjectRequest request, IProjectService service, CancellationToken ct)
    {
        var project = await service.CreateAsync(request, ct);
        // 201 Created plus a Location header pointing at the new resource (REST convention).
        return TypedResults.Created($"/api/projects/{project.Id}", ProjectResponse.FromModel(project));
    }

    private static async Task<Results<NoContent, NotFound>> Update(
        string id, ProjectRequest request, IProjectService service, CancellationToken ct)
    {
        return await service.UpdateAsync(id, request, ct)
            ? TypedResults.NoContent()
            : TypedResults.NotFound();
    }

    private static async Task<Results<NoContent, NotFound>> Delete(
        string id, IProjectService service, CancellationToken ct)
    {
        return await service.DeleteAsync(id, ct)
            ? TypedResults.NoContent()
            : TypedResults.NotFound();
    }
}
