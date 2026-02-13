using ContractChecker.Core.Models;
using Microsoft.Extensions.Configuration;

public class ApplicationProvider : IApplicationProvider
{
    public IEnumerable<ApplicationInfo> GetApplications(IConfiguration configuration)
    {
        return configuration

            .GetSection("TrackedApis")
            .GetChildren()
            .Select((section, index) => new ApplicationInfo
            {
                Name = section.Key,
                Id = index.ToString()
            })
            .ToList();
    }
}