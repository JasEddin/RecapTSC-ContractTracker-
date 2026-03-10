using ContractChecker.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using System.Text.RegularExpressions;

namespace ContractChecker.Core.Processor
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

            var contractFiles = latestFiles
                .Select(TryLoadContractFromOpenApiFile)
                .OfType<ContractFile>()
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
                //  Check if the file is empty 
                var reader = new OpenApiStreamReader();
                var doc = reader.Read(stream, out var diagnostics);

                (contractFile.Name, contractFile.Server) = GetNameAnddServer(filePath);

                contractFile.Team = NormalizeTeam(doc.Info?.Contact);

                contractFile.LatestVersion = GetVersionFromParentFolder(filePath) ?? 0;

                return contractFile;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private (string name, string server) GetNameAnddServer(string filePath)
        {
            var (name, server) = ("", "");
            // get the path of settings.yaml in the same folder of the json file
            var settingsFilePath = Path.Combine(Path.GetDirectoryName(filePath) ?? string.Empty, "settings.yaml");
            if (!File.Exists(settingsFilePath))
                return (name, server);

            // find properte's value of backendEndpoint: 
            var lines = File.ReadAllLines(settingsFilePath);
            
         
            foreach (var line in lines)
            {
                if (line.TrimStart().StartsWith("apicBasePath:"))
                {
                    // capitalize the Name and the letter after point in name, for example if the name is "user.profile" then the result should be "User.Profile"
                    var nameUncapitalized = line.Substring(line.IndexOf("apicBasePath::") + "apicBasePath::".Length).Trim().TrimStart('/');
                    name = string.Join('.', nameUncapitalized.Split('.').Select(part => char.ToUpper(part[0]) + part.Substring(1)));

                }
                if (line.TrimStart().StartsWith("backendEndpoint:"))
                {
                    server = line.Substring(line.IndexOf("backendEndpoint:") + "backendEndpoint:".Length).Trim();

                }
            }
            return (name, server);
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
        private Team NormalizeTeam(OpenApiContact? contact)
        {
            var unknownTeam = new Team
            {
                Name = "Unknown",
                Mail = "Unknown"
            };

            if (contact == null)
            {
                return unknownTeam;
            }
            if (string.IsNullOrWhiteSpace(contact.Name) || string.IsNullOrWhiteSpace(contact.Email))
                return unknownTeam;

            if (contact.Name.StartsWith("Team ", StringComparison.OrdinalIgnoreCase))
                return new Team { Name = contact.Name.Substring(5), Mail = contact.Email.Trim().ToLower() };

            return new Team { Name = contact.Name.Trim().ToLower(), Mail = contact.Email.Trim().ToLower() };
        }
    }
}
