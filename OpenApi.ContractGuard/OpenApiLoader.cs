using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

public class OpenApiLoader : IOpenApiLoader
{
    private readonly IHttpClientFactory _httpClientFactory;

    public OpenApiLoader(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

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

    public async Task<Uri?> ValidateServersAsync(IEnumerable<string> urls, int latestVersion)
    {
        var suffixes = new[] {
           //"/swagger/v1.0/swagger.json", 
           $"/openapi/v{latestVersion}/openapi.json",
           //"/openapi/v1.0/openapi.json", check it later
        };

        if (urls == null || !urls.Any())
            throw new ArgumentException("URL list must not be empty.", nameof(urls));

        var validUris = new List<Uri>();
        var client = _httpClientFactory.CreateClient("OpenApiProbe");

        foreach (var url in urls)
        {
            
            foreach(var suffix in suffixes)
            {
                var testUrl = url + suffix;
                if (string.IsNullOrWhiteSpace(testUrl))
                    continue;

                if (!IsValidAbsoluteUrl(testUrl, out var uri))
                    continue;

                var reachable = await IsReachableAsync(client, uri);
                if (reachable)
                    validUris.Add(uri);
            }
        }

        return validUris.LastOrDefault();
    }

    public async Task<OpenApiDocument> LoadFromServerAsync(IEnumerable<string> urls, int latestVersion)
    {
 
        // if the urks is empty
        if (!urls.Any())
            throw new ArgumentException("Url must not be empty.", nameof(urls));


        // extract only valid urls from the list

        var validUrl = await ValidateServersAsync(urls, latestVersion).ConfigureAwait(false);

        using var httpClient = new HttpClient();
 
        using var response = await httpClient.GetAsync(validUrl);

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

    public async Task<OpenApiDocument> LoadFromUrlAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Url must not be empty.", nameof(url));



        using var httpClient = new HttpClient();

        using var response = await httpClient.GetAsync(url);

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


    private static async Task<bool> IsReachableAsync(HttpClient client, Uri uri)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(0, 0);

            using var response = await client.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead
            );

            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
        catch (TaskCanceledException)
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
