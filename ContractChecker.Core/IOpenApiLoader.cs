using Microsoft.OpenApi.Models;

public interface IOpenApiLoader
{
    (OpenApiDocument? Document, string[] Errors) LoadFromPath(string path);
    Task<( OpenApiDocument? Document, string[] Errors)> LoadFromValidServerAsync(string server, int latestVersion, string env = "u3");
    Task<bool> ValidateServerAsync(string url);
}