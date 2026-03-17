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

    public OpenApiDocument? LoadFromPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            Console.WriteLine("[WARN] File path is empty.");
            return null;
        }

        if (!File.Exists(path))
        {
            Console.WriteLine($"[WARN] OpenAPI file not found: {path}");
            return null;
        }

        try
        {
            using var stream = File.OpenRead(path);
            var reader = new OpenApiStreamReader();

            var document = reader.Read(stream, out var diagnostics);

            if (diagnostics.Errors.Any())
            {
                Console.WriteLine($"[WARN] Invalid OpenAPI document: {path}");
                foreach (var error in diagnostics.Errors)
                {
                    Console.WriteLine($"   - {error.Message}");
                }
                return document;
            }

            if (document == null)
            {
                Console.WriteLine($"[WARN] Failed to parse OpenAPI document: {path}");
                return null;
            }

            return document;
        }
        catch (IOException ex)
        {
            Console.WriteLine($"[ERROR] IO error while reading: {path}");
            Console.WriteLine(ex.Message);
            return null;
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"[ERROR] No permission to read: {path}");
            Console.WriteLine(ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Unexpected error while loading: {path}");
            Console.WriteLine(ex.Message);
            return null;
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


    public async Task<OpenApiDocument?> LoadFromValidServerAsync(
     string server,
     int latestVersion,
     string environment = "u3")
    {
        if (string.IsNullOrWhiteSpace(server))
        {
            Console.WriteLine("[WARN] Server URL is empty.");
            return null;
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
                    Console.WriteLine($"[WARN] Invalid OpenAPI document at {url}");
                    continue;  // toDo return the error to UI
                }

                Console.WriteLine($"[INFO] Successfully loaded OpenAPI from {url}");
                return document;
            }

            catch (Exception)
            {

            }
        }
        Console.WriteLine($"[ERROR] No valid OpenAPI endpoint found for {server}");
        return null;
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
