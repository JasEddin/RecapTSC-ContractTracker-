
namespace ContractChecker.Core;

public class ContractDiffService
{
    public ContractDiffResult Compare(string oldSpec, string newSpec)
    {
        // your OpenAPI logic here
        return new ContractDiffResult
        {
        };
    }

    public ContractDiffResult? Compare(object oldSpec, object newSpec)
    {
        throw new NotImplementedException();
    }
}
