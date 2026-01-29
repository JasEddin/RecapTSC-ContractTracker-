using OpenApi.ContractGuard.Comparison.enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenApi.ContractGuard.Comparison
{
    public static class ChangeImpactRules
    {
        public static ChangeImpact GetImpact(ChangeType changeType)
        {
            return changeType switch
            {
                ChangeType.VersionChanged => ChangeImpact.ContractUpdateRequired,
                ChangeType.PathAdded => ChangeImpact.ContractUpdateRequired,
                ChangeType.OperationAdded => ChangeImpact.ContractUpdateRequired,
                ChangeType.PathRemoved => ChangeImpact.Informational,
                ChangeType.OperationRemoved => ChangeImpact.Informational,
                _ => ChangeImpact.Informational
            };
        }
    }
}
