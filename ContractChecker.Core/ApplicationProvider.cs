using ContractChecker.Core.Models;
using ContractChecker.Core.Proccor;
using Microsoft.Extensions.Configuration;
using OpenApi.ContractGuard.Comparison;
using OpenApi.ContractGuard.Comparison.enums;

public class ApplicationProvider : IApplicationProvider
{
    public Dictionary<string, (string url, string localContractPath, List<ContractChange> changes)> _apiAnddChanges = [];

    private bool _initialized;

    private IOpenApiLoader _openApiLoader;
    private IContractFileProvider _contractFileProvider;

    //public IConfiguration _configuration { get; }

    public ApplicationProvider(  IOpenApiLoader openApiLoader, IContractFileProvider contractFileProvider)
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
        var allFilesContracts = _contractFileProvider.LoadAllA();

        foreach (var fc in allFilesContracts)
        {
            var apiName = fc.Name;

                var file1 = await _openApiLoader.LoadFromUrlAsync( fc.Servers  ).ConfigureAwait(false);

                var file2 = _openApiLoader.LoadFromPath(fc.FilePathinApisFolder);

                var comparer = new OpenApiComparer();
                List<ContractChange> changes = comparer.Compare(file1, file2);

                _apiAnddChanges.Add(apiName, (fc.Servers[0], fc.FilePathinApisFolder , changes));
        }
        _initialized = true;
    }
}