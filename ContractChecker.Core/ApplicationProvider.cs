using ContractChecker.Core.Caching;
using ContractChecker.Core.Models;
using Microsoft.OpenApi.Models;
using OpenApi.ContractGuard.Comparison;
using OpenApi.ContractGuard.Comparison.enums;

public class ApplicationProvider : IApplicationProvider
{
    // Todo: make type ViewModel, add errors to it

    public Dictionary<string, ApplicationDetail> ApplicationDetailDic { get; set; } = [];
    private bool _initialized;
    private IOpenApiLoader _openApiLoader;
    private IContractFileProvider _contractFileProvider;

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
        var applicationChangeImpactList = ApplicationDetailDic.Select( kvp => new ApplicationInfo { Name = kvp.Key, Team = kvp.Value.Team, ChangeImpact = kvp.Value.Changes.Any(c => c.Impact == ChangeImpact.ContractUpdateRequired) ? ChangeImpact.ContractUpdateRequired : ChangeImpact.Informational });
        return applicationChangeImpactList;
    }

    public async Task<ApplicationDetail> GetApplicationAsync(string name)
    {
        if (!_initialized)
        {
            await ExtractChangesAsync();
        }

      return  ApplicationDetailDic[name];
 
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
                                            PathInApis = fc.PathInApis,
                                            LatestVersion = fc.LatestVersion,
                                            Server = kvp.Value, 
                                            Team = fc.Team
                                        };
        // Todo:  here already we return the method

        foreach (var fc in contractfilesWithValidServers )
        {
            OpenApiDocument? file1 = await _openApiLoader.LoadFromValidServerAsync(fc.Server, fc.LatestVersion).ConfigureAwait(false);

            OpenApiDocument? file2 = _openApiLoader.LoadFromPath(fc.PathInApis);
            if (file1 != null && file2 != null)
            {
                var comparer = new OpenApiComparer();
                List<ContractChange> changes = comparer.Compare(file1, file2);

                if (!ApplicationDetailDic.TryAdd(fc.Name, new ApplicationDetail {Name= fc.Name, Server = fc.Server, LocalContractPath= fc.PathInApis,Changes= changes, Team= fc.Team }))
                {
                    Console.WriteLine($"Warning: Duplicate API name '{fc.Name}' found. Skipping.");
                }
            }
            else
            {
                Console.WriteLine($"couldnt reach open API file from the server {fc.Server}' for the API: {fc.Name}.");
            }
            // Todo: if not ?? add error to the model and show it in the UI
        }
        _initialized = true;
    }


    async Task<Dictionary<string, string>> ExtractAllValidServersAsync(List<ContractFile> contractFiles)
    {

        //1️⃣ Try cache first
       var cached = await UrlCacheStorage.LoadAsync();

        if (cached != null)
        {
            return cached.ValidServers;
        }

        // 2️⃣ No cache → compute
        var result = new Dictionary<string, string>();
        var unvalidServers = new Dictionary<string, string>();

        foreach (var fc in contractFiles)
        {
            if (string.IsNullOrWhiteSpace(fc.Server))
                continue;

            var apiName = fc.Name;
            if (string.IsNullOrWhiteSpace(apiName))
                continue;
            
            List<string> urls = await _openApiLoader.ValidateServerAsync(fc.Server)
                                           .ConfigureAwait(false);

            if (urls.Count != 0 && urls.FirstOrDefault() != null)
                result[apiName] = urls.First();
            else
                unvalidServers[apiName] = fc.Server;
        }

        // 3️⃣ Save to cache
        var cache = new UrlValidationCache
        {
            ValidServers = result,
            UnvalidServers = unvalidServers,
            CreatedAtUtc = DateTime.UtcNow
        };
        await UrlCacheStorage.SaveAsync(cache);

        return result;

    }
}