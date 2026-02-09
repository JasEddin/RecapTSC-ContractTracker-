using OpenApi.ContractGuard.Comparison.enums;

namespace OpenApi.ContractGuard.Comparison
{
    public static class ChangeImpactRules
    {
        public static ChangeImpact GetImpact(ChangeType changeType)
        {
            return changeType switch
            {
                ChangeType.VersionChanged => ChangeImpact.Informational,
                ChangeType.PathAdded => ChangeImpact.ContractUpdateRequired,
                ChangeType.OperationAdded => ChangeImpact.ContractUpdateRequired,
                ChangeType.PathRemoved => ChangeImpact.ContractUpdateRequired,
                ChangeType.OperationRemoved => ChangeImpact.Informational,
                _ => ChangeImpact.Informational
            };
        }
    }
}
