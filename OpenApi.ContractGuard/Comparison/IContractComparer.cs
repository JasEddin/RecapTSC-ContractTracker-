using Microsoft.OpenApi.Models;

namespace OpenApi.ContractGuard.Comparison
{
    public interface IContractComparer
    {
        List<ContractChange> Compare(
            OpenApiDocument oldDoc,
            OpenApiDocument newDoc);
    }
}