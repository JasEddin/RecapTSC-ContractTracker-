using Microsoft.OpenApi.Models;

public interface IOpenApiLoader
{
    OpenApiDocument? LoadFromPath(string path);
    Task<OpenApiDocument> LoadFromUrlAsync(string url);
    Task<OpenApiDocument?> LoadFromValidServerAsync(string server, int latestVersion, string env = "u3");
    Task<bool> ValidateServerAsync(string url);
}