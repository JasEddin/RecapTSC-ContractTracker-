namespace OpenApi.ContractGuard.Services
{
    public interface IContractComparerService
    {
        Task RunAsync();
        void GetComparisonResults();
    }
}