using ContractChecker.Core.Comparison.enums;

namespace ContractChecker.Core.Comparison
{
    public class ContractChange
    {
        public ChangeType ChangeType { get; set; }

        public string Path { get; set; } = "";

        public string Operation { get; set; } = "";

        public string Message { get; set; } = "";
 
        public ChangeImpact Impact { get; set; }

        internal static ContractChange Create(
          ChangeType type,
          string message,
          string path = "",
          string operation = "")
        {
            return new ContractChange
            {
                ChangeType = type,
                Message = message,
                Path = path,
                Operation = operation,
                Impact = ChangeImpactRules.GetImpact(type)
            };
        }
    }
}
