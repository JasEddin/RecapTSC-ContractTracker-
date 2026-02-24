using Microsoft.OpenApi.Models;

public interface IOpenApiLoader
{
    OpenApiDocument LoadFromPath(string path);
    Task<OpenApiDocument> LoadFromUrlAsync(IEnumerable<string> url);
}