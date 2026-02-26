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

        var test =  await extractAllUrlsAsync(allFilesContracts);

        foreach (var fc in allFilesContracts)
        {
            var apiName = fc.Name;

            var file1 = await _openApiLoader.LoadFromUrlAsync(fc.Servers).ConfigureAwait(false);

            var file2 = _openApiLoader.LoadFromPath(fc.FilePathinApisFolder);

            var comparer = new OpenApiComparer();
            List<ContractChange> changes = comparer.Compare(file1, file2);

            _apiAnddChanges.Add(apiName, (fc.Servers[0], fc.FilePathinApisFolder, changes));
        }
        _initialized = true;
    }


    async Task<Dictionary<string, Uri>> extractAllUrlsAsync(List<ContractFile> contractFiles)
    {
        var result = new Dictionary<string, Uri>();
        var noServer = new Dictionary<string, string[]>();
        foreach (var fc in contractFiles)
        {
            var apiName = fc.Name;

            Uri url = await _openApiLoader.ValidateUrlsAsync(fc.Servers).ConfigureAwait(false);
            if (url != null)
            {
                result.TryAdd(apiName, url);
            }
            else { noServer.TryAdd(apiName, fc.Servers); }
        }

        List<string> noServerValues = noServer.SelectMany(s => s.Value).ToList();
        return result;  
    }
}