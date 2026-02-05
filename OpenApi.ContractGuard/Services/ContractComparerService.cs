using Microsoft.Extensions.Configuration;
using OpenApi.ContractGuard.Comparison;
using OpenApi.ContractGuard.Configurations;
using OpenApi.ContractGuard.Reporting;
    
namespace OpenApi.ContractGuard.Services
{
    internal class ContractComparerService
    {
        public IConfiguration _config { get; }

        public Dictionary<string, List<ContractChange>> _ApiAndChanges = new Dictionary<string, List<ContractChange>>();

        public ContractComparerService(IConfiguration config)
        {
            _config = config;
        }

        public void Run()
        {
            // Implementation for comparing contracts goes here
            IEnumerable<IConfigurationSection> trackedApis = _config.GetSection("TrackedApis").GetChildren();

            foreach (var ApiSection in trackedApis)
            {
                var apiName = ApiSection.Key;

                var apiConfig = ApiSection.Get<TrackedApiConfig>();

                if (apiConfig != null)
                {

                    var file1Task = new OpenApiLoader().LoadFromUrlAsync(apiConfig.Url);
                    var file2 = new OpenApiLoader().LoadFromPath(apiConfig.LocalContractPath);

                    var comparer = new OpenApiComparer();
                    List<ContractChange> changes = comparer.Compare(file1Task.Result, file2);

                    _ApiAndChanges.Add(apiName, changes);

                    ConsoleReporter.ShortReportChanges(apiName, changes);

                }
            }
        }
        
        public void GetComparisonResults()
        {
             ConsoleReporter.DetailedReportChanges(_ApiAndChanges);
        }

    }
}
 