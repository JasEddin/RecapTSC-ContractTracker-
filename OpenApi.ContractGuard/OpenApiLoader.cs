using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using System;

public class OpenApiLoader : IOpenApiLoader
{
    public OpenApiDocument LoadFromPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path must not be empty.", nameof(path));

        if (!File.Exists(path))
            throw new FileNotFoundException("OpenAPI file not found.", path);

        using var stream = File.OpenRead(path);
        var reader = new OpenApiStreamReader();

        var document = reader.Read(stream, out var diagnostics);

        if (diagnostics.Errors.Any())
        {
            throw new InvalidOperationException(
                "Invalid OpenAPI document:" + Environment.NewLine +
                string.Join(Environment.NewLine, diagnostics.Errors.Select(e => e.Message)));
        }

        if (document is null)
            throw new InvalidOperationException("Failed to parse OpenAPI document.");

        return document;
    }

    public async Task<OpenApiDocument> LoadFromUrlAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Url must not be empty.", nameof(url));

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            throw new ArgumentException("Invalid URL format.", nameof(url));

        using var httpClient = new HttpClient();
        using var response = await httpClient.GetAsync(uri);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(
                $"Failed to download OpenAPI document. Status: {response.StatusCode}");

        using var stream = await response.Content.ReadAsStreamAsync();
        var reader = new OpenApiStreamReader();

        var document = reader.Read(stream, out var diagnostics);

        if (diagnostics.Errors.Any())
        {
            throw new InvalidOperationException(
                "Invalid OpenAPI document:" + Environment.NewLine +
                string.Join(Environment.NewLine, diagnostics.Errors.Select(e => e.Message)));
        }

        if (document is null)
            throw new InvalidOperationException("Failed to parse OpenAPI document.");

        return document;
    }
}
