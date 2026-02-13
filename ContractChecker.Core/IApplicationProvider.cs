using ContractChecker.Core.Models;
using Microsoft.Extensions.Configuration;

public interface IApplicationProvider
{
     IEnumerable<ApplicationInfo> GetApplications(IConfiguration configuration);
}