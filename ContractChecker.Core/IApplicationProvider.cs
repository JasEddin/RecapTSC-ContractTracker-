using ContractChecker.Core.Models;

public interface IApplicationProvider
{
    Task<IEnumerable<ApplicationInfo>> GetApplicationsAsync();
    Task<IEnumerable<ApplicationInfo>> GetApplicationsAsync(string env);
    Task<ApplicationDetail> GetApplicationAsync(string name, string env);
}