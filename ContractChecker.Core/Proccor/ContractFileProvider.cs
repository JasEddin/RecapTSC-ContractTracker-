using ContractChecker.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Readers;
using System.Text.RegularExpressions;

namespace ContractChecker.Core.Proccor
{
    public class ContractFileProvider : IContractFileProvider
    {

        private readonly string _contractsPath;

        public ContractFileProvider(IConfiguration configuration)
        {
            var folder = configuration["ContractsFolder"]
                ?? throw new InvalidOperationException("ContractsFolder not configured");

            _contractsPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                folder
            );
        }
        public List<ContractFile> LoadAllLocalFiles()
        {
            if (!Directory.Exists(_contractsPath))
                return [];

            // 1. Find all json files (except settings.json)
            var files = Directory
                .GetFiles(_contractsPath, "*.json", SearchOption.AllDirectories)
                .Where(f => !f.EndsWith("settings.json", StringComparison.OrdinalIgnoreCase));

            // 2. Group by filename 
            var latestFiles = files
                .Select(f => new
                {
                    FullPath = f,
                    FileName = Path.GetFileName(f),
                    Version = GetVersionFromParentFolder(f)
                })
                // only consider files that actually have a version folder
                .Where(x => x.Version.HasValue)
                .GroupBy(x => x.FileName)
                // pick highest version
                .Select(g => g
                    .OrderByDescending(x => x.Version)
                    .First()
                    .FullPath)
                .ToList();

            List<ContractFile> contractFiles = latestFiles.Select(TryLoadContractFromOpenApiFile)
                                                   .Where(cf => cf != null)
                                                   .Cast<ContractFile>()
                                                   .ToList();
            return contractFiles;
        }

        // method to load a json file and extract the name and the server url from it, the json file has the following format:
        private ContractFile? TryLoadContractFromOpenApiFile(string filePath)
        {
            try
            {
                var contractFile = new ContractFile
                {
                    PathInApis = filePath
                };

                using var stream = File.OpenRead(filePath);
                var reader = new OpenApiStreamReader();
                var doc = reader.Read(stream, out var diagnostics);

                // change it ( sometimes the tittle is contains the version, we want to remove it) 
                contractFile.Name = doc.Info.Title;

                // extract servers from Settings.Yaml 
                contractFile.Server = GetServer(filePath);

                contractFile.LatestVersion = GetVersionFromParentFolder(filePath) ?? 0;

                return contractFile;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private string GetServer(string filePath)
        {
            // get the path of settings.yaml in the same folder of the json file
            var settingsFilePath = Path.Combine(Path.GetDirectoryName(filePath) ?? string.Empty, "settings.yaml");
            if (!File.Exists(settingsFilePath))
                return "";

            // find properte's value of backendEndpoint: 
            var lines = File.ReadAllLines(settingsFilePath);

            foreach (var line in lines)
            {
                if (line.TrimStart().StartsWith("backendEndpoint:"))
                {
                    var url = line.Substring(line.IndexOf("backendEndpoint:") + "backendEndpoint:".Length).Trim();
                    return url;
                }
            }
            return "";
        }

        private static int? GetVersionFromParentFolder(string filePath)
        {
            var parentFolder = Directory.GetParent(filePath)?.Name;

            if (parentFolder == null)
                return null;

            // Match v1, v2, v10, v123 ...
            var match = Regex.Match(parentFolder, @"^v(\d+)$", RegexOptions.IgnoreCase);

            return match.Success
                ? int.Parse(match.Groups[1].Value)
                : null;
        }
    }
}
