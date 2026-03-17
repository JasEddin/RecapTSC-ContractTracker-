using Microsoft.OpenApi.Models;

namespace ContractChecker.Core.Comparison
{
    public interface IContractComparer
    {
        List<ContractChange> Compare(
            OpenApiDocument oldDoc,
            OpenApiDocument newDoc);
    }
}