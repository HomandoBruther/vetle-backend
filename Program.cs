using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using VetleApi.Auth;
using VetleApi.Configuration;
using VetleApi.Endpoints;
using VetleApi.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------- Services (dependency injection registrations) ----------

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails(); // errors are returned as standard RFC 9457 JSON
builder.Services.AddValidation();     // checks [Required], [Url] and similar attributes on request DTOs

// Bind the "MongoDb" config section to MongoDbSettings and fail at startup if it is missing.
builder.Services.AddOptions<MongoDbSettings>()
    .Bind(builder.Configuration.GetSection(MongoDbSettings.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// MongoClient is thread-safe and holds a connection pool: create ONE for the whole app.
builder.Services.AddSingleton<IMongoClient>(sp =>
    new MongoClient(sp.GetRequiredService<IOptions<MongoDbSettings>>().Value.ConnectionString));

builder.Services.AddSingleton<IProjectService, ProjectService>();

// Authentication: identify the caller by the X-Api-Key header (see Auth/ApiKeyAuthenticationHandler.cs).
builder.Services.AddOptions<ApiKeySettings>()
    .Bind(builder.Configuration.GetSection(ApiKeySettings.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddAuthentication(ApiKeyAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
        ApiKeyAuthenticationHandler.SchemeName, configureOptions: null);

// Authorization: the "Owner" policy requires an authenticated caller (a valid API key).
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AuthPolicies.Owner, policy => policy.RequireAuthenticatedUser());

// Allow the Angular app to call the API from the browser. Origins come from config.
const string FrontendCorsPolicy = "Frontend";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
    options.AddPolicy(FrontendCorsPolicy, policy =>
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// ---------- HTTP pipeline (middleware runs in this order) ----------

app.UseExceptionHandler(); // unhandled exceptions become a 500 ProblemDetails, not a stack trace
app.UseStatusCodePages();  // empty 404/405 responses also get a ProblemDetails body

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // OpenAPI document at /openapi/v1.json
}

app.UseHttpsRedirection();
app.UseCors(FrontendCorsPolicy); // before auth, so browser preflight (OPTIONS) requests aren't rejected
app.UseAuthentication();         // who are you? (reads X-Api-Key)
app.UseAuthorization();          // are you allowed? (checks the endpoint's policy)

// ---------- Endpoints ----------

app.MapGet("/api/health", () => TypedResults.Ok(new { status = "ok" })).WithTags("Health");
app.MapProjectEndpoints();

app.Run();
