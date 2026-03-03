using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

public class OpenApiLoader : IOpenApiLoader
{
    private const string EnvironmentOFServer = "u3";

    private readonly IHttpClientFactory _httpClientFactory;

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

    public async Task<List<string>> ValidateServerAsync(string url)
    {
        var suffixes = new[] {
           //"/swagger/v1.0/swagger.json", 
           //$"/openapi/v{latestVersion}/openapi.json",
           //"/openapi/v1.0/openapi.json", check it later
           "/swagger/index.html"
        };

        // we need to replace the environment variable with the actual value
        url = url.Replace("$(environment)", $".{EnvironmentOFServer}.");

        if (string.IsNullOrWhiteSpace(url))
            return null;

        var validServers = new List<string>();
        var client = _httpClientFactory.CreateClient("OpenApiProbe");

        foreach (var suffix in suffixes)
        {
            var swaggerLink = url + suffix;
            if (string.IsNullOrWhiteSpace(swaggerLink))
                continue;

            if (!IsValidAbsoluteUrl(swaggerLink, out var uri))
                continue;

            var reachable = await IsReachableAsync(client, uri);
            if (reachable)
                validServers.Add(url);
        }

        return validServers;
    }

    //public async Task<OpenApiDocument> LoadFromServerAsync(IEnumerable<string> urls, int latestVersion)
    //{

    //    // if the urls is empty
    //    if (!urls.Any())
    //        throw new ArgumentException("Url must not be empty.", nameof(urls));

    //    // extract only valid urls from the list
    //    var validUrl = await ValidateServersAsync(urls).ConfigureAwait(false);

    //    using var httpClient = new HttpClient();

    //    using var response = await httpClient.GetAsync(validUrl);

    //    if (!response.IsSuccessStatusCode)
    //        throw new InvalidOperationException(
    //            $"Failed to download OpenAPI document. Status: {response.StatusCode}");

    //    using var stream = await response.Content.ReadAsStreamAsync();
    //    var reader = new OpenApiStreamReader();

    //    var document = reader.Read(stream, out var diagnostics);

    //    if (diagnostics.Errors.Any())
    //    {
    //        throw new InvalidOperationException(
    //            "Invalid OpenAPI document:" + Environment.NewLine +
    //            string.Join(Environment.NewLine, diagnostics.Errors.Select(e => e.Message)));
    //    }

    //    if (document is null)
    //        throw new InvalidOperationException("Failed to parse OpenAPI document.");

    //    return document;
    //}

    public async Task<OpenApiDocument?> LoadFromValidServerAsync(
     string server,
     int latestVersion)
    {
        if (string.IsNullOrWhiteSpace(server))
        {
            Console.WriteLine("[WARN] Server URL is empty.");
            return null;
        }

        //server = server.Replace("$(environment)", $".{EnvironmentOFServer}.");
        var candidateUrls = new[]
            { $"{server}/openapi/v{latestVersion}/openapi.json",
             $"{server}/swagger/v{latestVersion}/swagger.json" };
        //  $"{serverUrl}//openapi.json" (2 träff) , we can add it later if needed
        //openapi/v{latestVersion}.json (1 träff)  

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
                    continue;
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
