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

    public async Task<OpenApiDocument> LoadFromUrlAsync(IEnumerable<string> urls)
    {
         // if the urks is empty
            if ( !urls.Any() )         
            throw new ArgumentException("Url must not be empty.", nameof(urls));


        // extract only valid urls from the list

        Uri validUrls = await ValidateUrlsAsync(urls).ConfigureAwait(false);

        using var httpClient = new HttpClient();
        //// I want to impelent a certificate to the header
        ////of the request to be able to access the protected api, but for now I will just use the http client without it
        //var handler = new HttpClientHandler
        //{
        //    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        //};

        using var response = await httpClient.GetAsync(validUrls);

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

    public async Task<Uri> ValidateUrlsAsync(IEnumerable<string> urls)
    {
        var suffix = new[] {
       
            "/swagger/v1.0/swagger.json", 
            //"/swagger/v2.0/swagger.json",
            //"/swagger/v3.0/swagger.json",
            //"/swagger/v4.0/swagger.json",
            //"/swagger/v5.0/swagger.json",

            "/openapi/v1/openapi.json",
            //"/openapi/v3/openapi.json",
            //"/openapi/v3/openapi.json",
            //"/openapi/v4/openapi.json",
            //"/openapi/v5/openapi.json",
            //"/openapi/v6/openapi.json",
            //"/openapi/v7/openapi.json",

            "/openapi/v1.0/openapi.json",
            //"/openapi/v3.0/openapi.json",
            //"/openapi/v3.0/openapi.json",
            //"/openapi/v4.0/openapi.json",
            //"/openapi/v5.0/openapi.json",
            //"/openapi/v6.0/openapi.json",
            //"/openapi/v7.0/openapi.json",
        };

        if (urls == null || !urls.Any())
            throw new ArgumentException("URL list must not be empty.", nameof(urls));

        var validUris = new List<Uri>();
        var client = _httpClientFactory.CreateClient("OpenApiProbe");

        foreach (var url in urls)
        {
            
            foreach(var suf in suffix)
            {
                var testUrl = url + suf;
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
