namespace ContractChecker.Core.Models
{
    public class UrlValidationCache
    {
        public Dictionary<string, List<string>> ValidServers { get; set; } = new Dictionary<string, List<string>>();
        public Dictionary<string, string[]> UnvalidServers { get; set; } = [];
        public DateTime CreatedAtUtc { get; set; }
    }
}
