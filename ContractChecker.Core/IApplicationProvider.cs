using ContractChecker.Core.Models;
using Microsoft.Extensions.Configuration;

public interface IApplicationProvider
{
    Task<IEnumerable<ApplicationInfo>> GetApplicationsAsync();
    Task<ApplicationDetail> GetApplicationAsync(string id);
}