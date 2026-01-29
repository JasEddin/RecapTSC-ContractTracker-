using System;
using System.Collections.Generic;
using System.Text;

namespace OpenApi.ContractGuard.Comparison.enums
{
    public enum ChangeType
    {
        VersionChanged,

        PathAdded,
        PathRemoved,

        OperationAdded,
        OperationRemoved
    }
}
