using ContractChecker.Core.Caching;
using ContractChecker.Core.Models;
using Microsoft.OpenApi.Models;
using OpenApi.ContractGuard.Comparison;
using OpenApi.ContractGuard.Comparison.enums;

public class ApplicationProvider : IApplicationProvider
{
    // Todo: make type ViewModel, add errors to it
    public Dictionary<string, (string url, string localContractPath, List<ContractChange> changes)> _apiAnddChanges = [];

    private bool _initialized;

    private IOpenApiLoader _openApiLoader;
    private IContractFileProvider _contractFileProvider;

    //public IConfiguration _configuration { get; }

    public ApplicationProvider(IOpenApiLoader openApiLoader, IContractFileProvider contractFileProvider)
    {
        _openApiLoader = openApiLoader;
        _contractFileProvider = contractFileProvider;

    }

    public async Task<IEnumerable<ApplicationInfo>> GetApplicationsAsync()
    {
        if (!_initialized)
        {
            await ExtractChangesAsync();
        }

        // return a list of applications name and index 
        var applicationChangeImpactList = _apiAnddChanges.Select(api => new ApplicationInfo { Name = api.Key, ChangeImpact = api.Value.changes.Any(c => c.Impact == ChangeImpact.ContractUpdateRequired) ? ChangeImpact.ContractUpdateRequired : ChangeImpact.Informational });
        return applicationChangeImpactList;
    }

    public async Task<ApplicationDetail> GetApplicationAsync(string name)
    {
        if (!_initialized)
        {
            await ExtractChangesAsync();
        }

        var (url, localContractPath, changes) = _apiAnddChanges[name];
        return new ApplicationDetail
        {
            Name = name,
            Url = url,
            LocalContractPath = localContractPath,
            Changes = changes
        };

    }

    async Task ExtractChangesAsync()
    {

        List<ContractFile> allFilesContracts = _contractFileProvider.LoadAllLocalFiles();

        var appServerdict =  await ExtractAllValidServersAsync(allFilesContracts);

        // group by api name and compare contract files
        IEnumerable<ContractFile> contractfilesWithValidServers = from fc in allFilesContracts
                                        join kvp in appServerdict on fc.Name equals kvp.Key
                                        select new ContractFile
                                        {
                                            Name = fc.Name,
                                            FilePathinApisFolder = fc.FilePathinApisFolder,
                                            LatestVersion = fc.LatestVersion,
                                            Servers = fc.Servers, 
                                        };
        // Todo:  here already we return the method



        foreach (var fc in contractfilesWithValidServers )
        {
            OpenApiDocument? file1 = await _openApiLoader.LoadFromValidServerAsync(fc.Servers, fc.LatestVersion).ConfigureAwait(false);

            OpenApiDocument? file2 = _openApiLoader.LoadFromPath(fc.FilePathinApisFolder);
            if (file1 != null && file2 != null)
            {
                var comparer = new OpenApiComparer();
                List<ContractChange> changes = comparer.Compare(file1, file2);

                if (!_apiAnddChanges.TryAdd(fc.Name, (fc.Servers[0], fc.FilePathinApisFolder, changes)))
                {
                    Console.WriteLine($"Warning: Duplicate API name '{fc.Name}' found. Skipping.");

                }
            }
            else
            {
                Console.WriteLine($"couldnt reach open API file from the server {fc.Servers[0]}' for the API: {fc.Name}.");
            }
            // Todo: if not ?? add error to the model and show it in the UI
        }
        _initialized = true;
    }


    async Task<Dictionary<string, List<string>>> ExtractAllValidServersAsync(List<ContractFile> contractFiles)
    {

        // 1️⃣ Try cache first
        var cached = await UrlCacheStorage.LoadAsync();

        if (cached != null)
        {
            return cached.AppValidUrl;
        }

        // 2️⃣ No cache → compute
        var result = new Dictionary<string, List<string>>();
        var noServer = new Dictionary<string, string[]>();

        foreach (var fc in contractFiles)
        {
            var apiName = fc.Name;

            List<string> urls = await _openApiLoader.ValidateServersAsync(fc.Servers)
                                           .ConfigureAwait(false);

            if (urls.Any())
                result[apiName] = urls;
            else
                noServer[apiName] = fc.Servers;
        }

        // 3️⃣ Save to cache
        var cache = new UrlValidationCache
        {
            AppValidUrl = result,
            NoServer = noServer,
            CreatedAtUtc = DateTime.UtcNow
        };
        await UrlCacheStorage.SaveAsync(cache);

        return result;

    }
}