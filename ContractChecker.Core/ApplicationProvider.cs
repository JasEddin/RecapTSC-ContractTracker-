using ContractChecker.Core.Models;
using Microsoft.Extensions.Configuration;
using OpenApi.ContractGuard.Comparison;

public class ApplicationProvider : IApplicationProvider
{
    public Dictionary<string, (string url, string localContractPath, List<ContractChange> changes)> _apiAnddChanges = [];
   
    private bool _initialized;
    
    private IOpenApiLoader _openApiLoader;
    
    public IConfiguration _configuration { get; }

    public ApplicationProvider(IConfiguration configuration, IOpenApiLoader openApiLoader)
    {
        _openApiLoader=openApiLoader;
        _configuration = configuration;
    }

    public async Task<IEnumerable<ApplicationInfo>> GetApplicationsAsync()
    {
        if (!_initialized)
        {
            await ExtractChangesAsync();
        }
        // return a list of applications name and index 
        return _apiAnddChanges.Select((kvp, index) => new ApplicationInfo
        {
            Id = index.ToString(),
            Name = kvp.Key
        });

    }

    public async Task<ApplicationDetail> GetApplicationAsync( string id)
    {
        if (!_initialized)
        {
            await ExtractChangesAsync();
        }

        // return the changes for the application with the given id
        if (int.TryParse(id, out int index) && index >= 0 && index < _apiAnddChanges.Count)
        {
            var apiName = _apiAnddChanges.ElementAt(index).Key;

            var (url, localContractPath, changes) = _apiAnddChanges[apiName];
            return new ApplicationDetail
            {
                Name = apiName,
                Url = url,
                LocalContractPath = localContractPath,
                Changes = changes
            };
        }
        else
        {
            throw new ArgumentException("Invalid application ID.", nameof(id));
        }

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