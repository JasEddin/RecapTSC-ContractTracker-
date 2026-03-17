using ContractChecker.Core.Comparison.enums;

namespace ContractChecker.Core.Models
{
    public class ApplicationInfo
    {
        public ChangeImpact ChangeImpact { get; set; } = ChangeImpact.Informational;

        public string Name { get; set; } = default!;

        public Team Team { get; set; }
    }
}
