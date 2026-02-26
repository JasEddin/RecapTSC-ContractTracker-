namespace ContractChecker.Core.Models
{
    public class UrlValidationCache
    {
        public Dictionary<string, List<string>> AppValidUrl { get; set; } = new Dictionary<string, List<string>>();
        public Dictionary<string, string[]> NoServer { get; set; } = [];
        public DateTime CreatedAtUtc { get; set; }
    }
}
