using OpenApi.ContractGuard.Comparison;

public class ApplicationDetail
{
    public string Name { get; set; }
    public string Url { get; set; }
    public string LocalContractPath { get; set; }
    public List<ContractChange> Changes { get; set; }
}