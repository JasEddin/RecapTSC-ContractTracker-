namespace ContractChecker.Core.Models
{
    public class UrlValidationCache
    {
        public Dictionary<string, string> ValidServers { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> UnvalidServers { get; set; } = new Dictionary<string, string>();
        public DateTime CreatedAtUtc { get; set; }
    }
}
