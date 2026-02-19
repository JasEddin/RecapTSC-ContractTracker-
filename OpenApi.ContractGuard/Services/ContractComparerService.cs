using Microsoft.Extensions.Configuration;
using OpenApi.ContractGuard.Comparison;
using OpenApi.ContractGuard.Configurations;
using OpenApi.ContractGuard.Reporting;

namespace OpenApi.ContractGuard.Services
{
    internal class ContractComparerService : IContractComparerService
    {
        public readonly IConfiguration _config;

        public Dictionary<string, List<ContractChange>> _ApiAndChanges = new();

        public ContractComparerService(IConfiguration config)
        {
            _config = config;
        }


        public async Task RunAsync()
        {
            try
            {

                // Implementation for comparing contracts goes here
                IEnumerable<IConfigurationSection> trackedApis = _config.GetSection("TrackedApis").GetChildren();

                foreach (var ApiSection in trackedApis)
                {
                    var apiName = ApiSection.Key;

                    var apiConfig = ApiSection.Get<TrackedApiConfig>();

                    if (apiConfig != null)
                    {

                        var file1 = await new OpenApiLoader().LoadFromUrlAsync(apiConfig.Url).ConfigureAwait(false);

                        var file2 = new OpenApiLoader().LoadFromPath(apiConfig.LocalContractPath);

                        var comparer = new OpenApiComparer();
                        List<ContractChange> changes = comparer.Compare(file1, file2);

                        _ApiAndChanges.Add(apiName, changes);

                        ConsoleReporter.ShortReportChanges(apiName, changes);

                    }
                }

            }
            catch (Exception ex)
            {
                ConsoleReporter.WriteError($"An error occurred during contract comparison: {ex.Message}");
            }
        }

        public void GetComparisonResults()
        {
            ConsoleReporter.DetailedReportChanges(_ApiAndChanges);
        }
    }
}
