using ContractChecker.Core.Caching;
using ContractChecker.Core.Comparison;
using ContractChecker.Core.Comparison.enums;
using ContractChecker.Core.Models;
using Microsoft.OpenApi.Models;

public class ApplicationProvider(IOpenApiLoader openApiLoader, IContractFileProvider contractFileProvider) : IApplicationProvider
{
    // Todo: make type ViewModel, add errors to it
    public Dictionary<string, Dictionary<string, ApplicationDetail>> _envAppsDict= [];
    private readonly IOpenApiLoader _openApiLoader = openApiLoader;
    private readonly IContractFileProvider _contractFileProvider = contractFileProvider;
    private IEnumerable<ContractFile>? _contractfilesWithValidServers;

    public async Task<IEnumerable<ApplicationInfo>> GetApplicationsAsync(string env)
    {
        await InitializeScanningApisFilesAsync(env);
        await ExtractChangesAsync(env);
        // return a list of applications name and index 
        var applicationChangeImpactList = _envAppsDict[env].Select(kvp => new ApplicationInfo { Name = kvp.Key, Team = kvp.Value.Team, ChangeImpact = kvp.Value.Changes.Any(c => c.Impact == ChangeImpact.ContractUpdateRequired) ? ChangeImpact.ContractUpdateRequired : ChangeImpact.Informational });
        return applicationChangeImpactList;
    }

    public async Task<ApplicationDetail> GetApplicationAsync(string name, string env)
    {
        await InitializeScanningApisFilesAsync(env);
        await ExtractChangesAsync(env);
        return _envAppsDict[env][name];
    }

    async Task ExtractChangesAsync(string env)
    {
        // check of _envApps has the same environment and if yes return the ApplicationDetailDic from it
        if (_envAppsDict.ContainsKey(env))
        {
            return;
        }

        var envAppsPair = (env, new Dictionary<string, ApplicationDetail>());

        // check _contractfilesWithValidServers is not null and if it is null initialize it by scanning the local files and validating the servers, if it is not null use it to extract the changes
   
        foreach (ContractFile fc in _contractfilesWithValidServers)
        {
            OpenApiDocument? file1 = await _openApiLoader.LoadFromValidServerAsync(fc.Server, fc.LatestVersion, env).ConfigureAwait(false);

            OpenApiDocument? file2 = _openApiLoader.LoadFromPath(fc.PathInApis);
            if (file1 != null && file2 != null)
            {
                var comparer = new OpenApiComparer(); // use DI to inject the comparer if it has dependencies

                List<ContractChange> changes = comparer.Compare(file1, file2);

                if (!envAppsPair.Item2.TryAdd(fc.Name, new ApplicationDetail { Name = fc.Name, Server = fc.Server, LocalContractPath = fc.PathInApis, Changes = changes, Team = fc.Team }))
                {
                    Console.WriteLine($"Warning: Duplicate API name '{fc.Name}' found. Skipping.");
                }
            }
            else
            {
                // Todo add error to UI to view the if openApi form is incorrect
                Console.WriteLine($"couldnt reach open API file from the server {fc.Server}' for the API: {fc.Name}.");
            }
            // Todo: if not ?? add error to the model and show it in the UI
        }
        _envAppsDict.TryAdd(env, envAppsPair.Item2);
    }

    private async Task InitializeScanningApisFilesAsync(string env)
    {
        if (_contractfilesWithValidServers != null) { return; }

        List<ContractFile> allFilesContractsInApis = _contractFileProvider.LoadAllLocalFiles();
        // stop here and return list of all applications unrelated to validServers.

        var appServerdict = await ExtractAllValidServersAsync(allFilesContractsInApis, env);

        _contractfilesWithValidServers = from fc in allFilesContractsInApis
                                         join kvp in appServerdict on fc.Name equals kvp.Key
                                         select new ContractFile
                                         {
                                             Name = fc.Name,
                                             PathInApis = fc.PathInApis,
                                             LatestVersion = fc.LatestVersion,
                                             Server = kvp.Value,
                                             Team = fc.Team
                                         };

    }

    async Task<Dictionary<string, string>> ExtractAllValidServersAsync(List<ContractFile> contractFiles, string env)
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
                                                                  // todo pass env here and change caching 
            var isValid = await _openApiLoader.ValidateServerAsync(fc.Server)
                                           .ConfigureAwait(false);

            if (isValid)
                result[apiName] = fc.Server;

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