using ContractChecker.Core.Models;
using OpenApi.ContractGuard.Comparison;

public class ApplicationDetail
{
    public string Name { get; set; }
    public string Server { get; set; }
    public string LocalContractPath { get; set; }
    public Team Team { get; set; }
    public List<ContractChange> Changes { get; set; }
}