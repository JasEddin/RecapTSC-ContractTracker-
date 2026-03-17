using System;
using System.Collections.Generic;
using System.Text;

namespace ContractChecker.Core.Comparison.enums
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
    