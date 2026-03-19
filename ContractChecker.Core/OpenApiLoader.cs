using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

public class OpenApiLoader : IOpenApiLoader
{
    private readonly IHttpClientFactory _httpClientFactory;
    private static IEnumerable<string> _environments = ["u3", "u4","u5"];

    public OpenApiLoader(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public ( OpenApiDocument? Document, string[] Errors) LoadFromPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
                return (null, [ "File path is empty." ]);
        }

        if (!File.Exists(path))
        {
            return (null, [ "File not found." ]);
        }

        try
        {
            using var stream = File.OpenRead(path);
            var reader = new OpenApiStreamReader();

            var document = reader.Read(stream, out var diagnostics);

            if (diagnostics.Errors.Any())
            {
                return  (document, diagnostics.Errors.Select(e => $"Invalid OpenAPI document: {e.Message}").ToArray());
            }

            if (document == null)
            {
                return (null, [ "Failed to parse OpenAPI document." ]);
            }

            return (document, Array.Empty<string>());
        }
        catch (IOException ex)
        {
            return (null, [ $"IO error while reading: {ex.Message}" ]);
        }
        catch (UnauthorizedAccessException ex)
        {
            return (null, [ $"No permission to read: {path}", ex.Message ]);
        }
        catch (Exception ex)
        {
            return (null, [ $"Unexpected error: {ex.Message}" ]);
        }
    }

    public async Task<bool> ValidateServerAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;
        foreach (var env in _environments)
        {
            var swaggerLink = url.Replace("$(environment)", $".{env}.") + "/swagger/index.html";
            var client = _httpClientFactory.CreateClient("OpenApiProbe");


            if (!IsValidAbsoluteUrl(swaggerLink, out var uri))
            {
                Console.WriteLine($"[WARN] Invalid URL format: {swaggerLink}");
                return false;
            }

            if (await IsReachableAsync(client, uri))
            {
                return true;
            }
        }
        return false;
    }


    public async Task<( OpenApiDocument? Document, string[] Errors)> LoadFromValidServerAsync(
     string server,
     int latestVersion,
     string environment = "u3")
    {
        if (string.IsNullOrWhiteSpace(server))
        {
            Console.WriteLine("[WARN] Server URL is empty.");
            return (null, ["Server URL is empty."]);
        }
        server = server.Replace("$(environment)", $".{environment}.");
        var candidateUrls = new[]
        {
        $"{server}/openapi/v{latestVersion}/openapi.json",
        $"{server}/swagger/v{latestVersion}/swagger.json",
        $"{server}/api-docs/v{latestVersion}.json",
        $"{server}/openapi.json"
        };

        using var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(3)
        };

        foreach (var url in candidateUrls)
        {
            try
            {
                using var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[WARN] {url} returned {response.StatusCode}");
                    continue;
                }

                using var stream = await response.Content.ReadAsStreamAsync();
                var reader = new OpenApiStreamReader();

                var document = reader.Read(stream, out var diagnostics);

                if (diagnostics.Errors.Any())
                {
                    
                    if (document.Paths == null) 
                       continue;

                    return (document, diagnostics.Errors.Select(e => $"Invalid OpenAPI document at {url}: {e.Message}").ToArray());
                }

                return (document, Array.Empty<string>());
            }

            catch (Exception ex)
            {
                return (null, new[] { $"Unexpected error: {ex.Message}" });
            }
        }
        return (null, new[] { "No valid OpenAPI endpoint found for server: " + server });
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
