using ContractChecker.Core.Models;
using Microsoft.Extensions.Configuration;
using OpenApi.ContractGuard.Comparison;
using OpenApi.ContractGuard.Comparison.enums;

public class ApplicationProvider : IApplicationProvider
{
    public Dictionary<string, (string url, string localContractPath, List<ContractChange> changes)> _apiAnddChanges = [];

    private bool _initialized;

    private IOpenApiLoader _openApiLoader;

    public IConfiguration _configuration { get; }

    public ApplicationProvider(IConfiguration configuration, IOpenApiLoader openApiLoader)
    {
        _openApiLoader = openApiLoader;
        _configuration = configuration;
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
        var trackedApis = _configuration.GetSection("TrackedApis").GetChildren();
        foreach (var ApiSection in trackedApis)
        {
            var apiName = ApiSection.Key;

            var apiConfig = ApiSection.Get<TrackedApiConfig>();

            if (apiConfig != null)
            {
                var file1 = await _openApiLoader.LoadFromUrlAsync(apiConfig.Url).ConfigureAwait(false);

                var file2 = _openApiLoader.LoadFromPath(apiConfig.LocalContractPath);

                var comparer = new OpenApiComparer();
                List<ContractChange> changes = comparer.Compare(file1, file2);

                _apiAnddChanges.Add(apiName, (apiConfig.Url, apiConfig.LocalContractPath, changes));
            }
        }
        _initialized = true;
    }
}