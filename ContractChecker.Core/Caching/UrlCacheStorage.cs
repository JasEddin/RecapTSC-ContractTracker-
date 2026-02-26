using ContractChecker.Core.Models;
using System.Text.Json;

namespace ContractChecker.Core.Caching
{
    static class UrlCacheStorage
    {
        private static readonly string CachePath =
            Path.Combine(AppContext.BaseDirectory, "url-cache.json");

        public static bool Exists() => File.Exists(CachePath);

        public static async Task SaveAsync(UrlValidationCache cache)
        {
            var json = JsonSerializer.Serialize(
                cache,
                new JsonSerializerOptions { WriteIndented = true }
            );

            await File.WriteAllTextAsync(CachePath, json);
        }

        public static async Task<UrlValidationCache?> LoadAsync()
        {
            if (!Exists())
                return null;

            var json = await File.ReadAllTextAsync(CachePath);
            return JsonSerializer.Deserialize<UrlValidationCache>(json);
        }
    }
}
