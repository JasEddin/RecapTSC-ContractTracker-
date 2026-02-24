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

    public async Task<OpenApiDocument> LoadFromUrlAsync(IEnumerable<string> urls)
    {
         // if the urks is empty
            if ( !urls.Any() )         
            throw new ArgumentException("Url must not be empty.", nameof(urls));


        // extract only valid urls from the list

        List<Uri> validUrls = await ValidateUrlsAsync(urls).ConfigureAwait(false);



        using var httpClient = new HttpClient();
        using var response = await httpClient.GetAsync(validUrls.First());

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




    public async Task<List<Uri>> ValidateUrlsAsync(IEnumerable<string> urlss)
    {
        var urls = new[] { "https://rsia.u3.skandianet.org/riskbedomning.rest","https://rest.u4.skandianet.org/insuranceprotect/"};

        if (urls == null || !urls.Any())
            throw new ArgumentException("URL list must not be empty.", nameof(urls));

        var validUris = new List<Uri>();

        using var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(5)
        };

        foreach (var url in urls)
        {
            if (string.IsNullOrWhiteSpace(url))
                continue;

            if (!IsValidAbsoluteUrl(url, out var uri))
                continue;

            var reachable = await IsReachableAsync(httpClient, uri);
            if (reachable)
                validUris.Add(uri);
        }

        return validUris;
    }

    private static async Task<bool> IsReachableAsync(HttpClient httpClient, Uri uri)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Head, uri);
            using var response = await httpClient.SendAsync(request);

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
    private static bool IsValidAbsoluteUrl(string url, out Uri uri)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out uri)
               && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
