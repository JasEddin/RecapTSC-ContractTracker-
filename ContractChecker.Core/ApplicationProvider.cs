using ContractChecker.Core.Caching;
using ContractChecker.Core.Comparison;
using ContractChecker.Core.Comparison.enums;
using ContractChecker.Core.Models;
using Microsoft.OpenApi.Models;

public class ApplicationProvider : IApplicationProvider
{
    // Todo: make type ViewModel, add errors to it
    public Dictionary<string, Dictionary<string, ApplicationDetail>> _envAppsDict = [];
    private readonly IOpenApiLoader _openApiLoader;
    private List<ContractFile> _allFilesContractsInApis;
    private readonly IContractComparer _comparer;

    public ApplicationProvider(IOpenApiLoader openApiLoader, IContractFileProvider contractFileProvider, IContractComparer comparer)
    {
        _openApiLoader = openApiLoader;
        _allFilesContractsInApis = contractFileProvider.LoadAllLocalFiles();
        _comparer = comparer;
    }

    public async Task<IEnumerable<ApplicationInfo>> GetApplicationsAsync()
    {
        return _allFilesContractsInApis.Select(fc => new ApplicationInfo { Name = fc.Name, Team = fc.Team, ChangeImpact = ChangeImpact.Unknown }).ToList();
    }

    public async Task<IEnumerable<ApplicationInfo>> GetApplicationsAsync(string env)
    {                                               
        await ExtractChangesAsync(env);
        // return a list of applications name and index 
        var applicationChangeImpactList = _envAppsDict[env].Select(kvp => new ApplicationInfo { Name = kvp.Key, Team = kvp.Value.Team,
            ChangeImpact = getImpact(kvp.Value.Changes),
            Errors = kvp.Value.Errors
        });
        return applicationChangeImpactList;
    }

    private ChangeImpact getImpact(List<ContractChange>? changes)
    {
        if (changes == null)
        {
            return ChangeImpact.Unknown;
        }

        if (!changes.Any())
        {
            return ChangeImpact.Unknown;
        }

        if (changes.Any(c => c.Impact == ChangeImpact.ContractUpdateRequired))
        {
            return ChangeImpact.ContractUpdateRequired;
        }

        return ChangeImpact.Informational;
    }

    public async Task<ApplicationDetail> GetApplicationAsync(string name, string env)
    {
        await ExtractChangesAsync(env).ConfigureAwait(false);
        return _envAppsDict[env][name];
    }

    async Task ExtractChangesAsync(string env)
    {
        // check of _envApps has the same environment and if yes return the ApplicationDetailDic from it
        if (_envAppsDict.ContainsKey(env))
        { return; }

        var envAppsPair = (env, new Dictionary<string, ApplicationDetail>());

        var appServersPair = await getServersStatus().ConfigureAwait(false);

        var validServers = appServersPair.ValidServers;
        var unvalidServers = appServersPair.UnvalidServers;

        List<ContractFile> _contractfilesWithValidServers = (from fc in _allFilesContractsInApis
                                                             join kvp in validServers on fc.Name equals kvp.Key
                                                             select new ContractFile
                                                             {
                                                                 Name = fc.Name,
                                                                 PathInApis = fc.PathInApis,
                                                                 LatestVersion = fc.LatestVersion,
                                                                 Server = kvp.Value,
                                                                 Team = fc.Team,
                                                                 Errors = fc.Errors
                                                             }).ToList();





        var contractFilesWithInvalidServersNames = appServersPair.UnvalidServers.Select(fc => fc.Key).ToHashSet();

        var contractFilesWithInvalidServersNamesAndApplicationDetails =
            contractFilesWithInvalidServersNames.ToDictionary(name => name, name =>
            {
                var fc = _allFilesContractsInApis.First(f => f.Name == name);
                return new ApplicationDetail { Name = fc.Name, Server = fc.Server, LocalContractPath = fc.PathInApis, Changes = null, Team = fc.Team,
                    Errors = new List<Error> { new Error { Message = $"Invalid server: {fc.Server}" } } };
            });

        foreach (ContractFile fc in _contractfilesWithValidServers)
        {
            var (file1, errors1) = await _openApiLoader.LoadFromValidServerAsync(fc.Server, fc.LatestVersion, env).ConfigureAwait(false);

            var (file2, errors2) = _openApiLoader.LoadFromPath(fc.PathInApis);

            if (file1 != null && file2 != null)
            {
                var changes = _comparer.Compare(file1, file2);

                if (!envAppsPair.Item2.TryAdd(fc.Name, new ApplicationDetail { Name = fc.Name, Server = fc.Server, LocalContractPath = fc.PathInApis, Changes = changes, Team = fc.Team, Errors = errors1.Concat(errors2).Select(x => new Error { Message = x }).ToList() }))
                {
                    Console.WriteLine($"Warning: Duplicate API name '{fc.Name}' found. Skipping.");
                }
            }
            else
            {
                envAppsPair.Item2.TryAdd(fc.Name, new ApplicationDetail { Name = fc.Name, Server = fc.Server, LocalContractPath = fc.PathInApis, Changes = new List<ContractChange>(), Team = fc.Team, Errors = errors1.Concat(errors2).Select(x => new Error { Message = x }).ToList() });
            }
        }

        foreach (var kvp in contractFilesWithInvalidServersNamesAndApplicationDetails)
        {
            envAppsPair.Item2.TryAdd(kvp.Key, kvp.Value);

            _envAppsDict.TryAdd(env, envAppsPair.Item2);
        }

        async Task<(Dictionary<string, string> ValidServers, Dictionary<string, string> UnvalidServers)> getServersStatus()
        {
            //1️⃣ Try cache first
            var cached = await UrlCacheStorage.LoadAsync();

            if (cached != null)
            {
                return (cached.ValidServers, cached.UnvalidServers);
            }

            // 2️⃣ No cache → compute
            var validServers = new Dictionary<string, string>();
            var unvalidServers = new Dictionary<string, string>();

            foreach (var fc in _allFilesContractsInApis)
            {
                if (string.IsNullOrWhiteSpace(fc.Server))
                    continue;

                var apiName = fc.Name;
                if (string.IsNullOrWhiteSpace(apiName))
                    continue;

                var isValid = await _openApiLoader.ValidateServerAsync(fc.Server)
                                               .ConfigureAwait(false);

                if (isValid)
                    validServers[apiName] = fc.Server;

                else
                    unvalidServers[apiName] = fc.Server;
            }

            // 3️⃣ Save to cache
            var cache = new UrlValidationCache
            {
                ValidServers = validServers,
                UnvalidServers = unvalidServers,
                CreatedAtUtc = DateTime.UtcNow
            };
            await UrlCacheStorage.SaveAsync(cache);

            return (cache.ValidServers, cache.UnvalidServers);
        }
    }
}