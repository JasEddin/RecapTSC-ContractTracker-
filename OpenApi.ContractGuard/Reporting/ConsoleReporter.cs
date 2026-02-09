using OpenApi.ContractGuard.Comparison;
using OpenApi.ContractGuard.Comparison.enums;

namespace OpenApi.ContractGuard.Reporting
{
    public static class ConsoleReporter
    {
        public static void DetailedReportChanges(Dictionary<string, List<ContractChange>> apiAndChanges)
        {
            foreach (var apiChanges in apiAndChanges)
            {
                Console.WriteLine();
                Console.ReadKey();
                var apisName = apiChanges.Key;
                var changes = apiChanges.Value;
                var spaceTabes = new string(' ', 15);
                Console.WriteLine(new string('=', 80));
                Console.WriteLine($"API: {spaceTabes + apisName}");
                Console.WriteLine(new string('=', 80));

                if (!changes.Any())
                {
                    WriteSuccess("No contract changes detected.");
                    continue;
                }

                if (changes.Any(change => change.Impact == ChangeImpact.ContractUpdateRequired))
                {
                    WriteCriticalBanner();
                }
                else
                {
                    WriteSuccess("Result: No contract update required.");
                }

                WriteInfo("Detected Changes:");

                foreach (var change in changes)
                {
                    WriteChange(change);
                }
                Console.WriteLine();
            }
        }

        public static void ShortReportChanges(string apisName, IEnumerable<ContractChange> changes)
        {
            if (!changes.Any())
            {
                WriteSuccess($"{apisName}: No contract changes detected.");
                return;
            }
            if (changes.Any(change => change.Impact == ChangeImpact.ContractUpdateRequired))
            {
                WriteError($"{apisName}: CRITICAL - Contract changes detected that require an update to the contract.");
            }
            else
            {
                WriteSuccess($"{apisName}: No contract update required.");
            }

        }

        public static void WriteError(string message)
        {
            var previousColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = previousColor;
        }

        private static void WriteCriticalBanner()
        {
            var title = "CRITICAL: Contract changes detected that require an update to the contract.";
            var message = "Please review the changes and update the contract accordingly.";
            var previousColor = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(new string('=', 80));
            Console.WriteLine(title);
            Console.WriteLine(new string('-', 80));
            Console.WriteLine(message);
            Console.WriteLine(new string('=', 80));

            Console.ForegroundColor = previousColor;
        }

        private static void WriteChange(ContractChange change)
        {
            switch (change.Impact)
            {
                case ChangeImpact.ContractUpdateRequired:
                    WriteError($"[BREAKING] {change.Message}");
                    break;
                case ChangeImpact.Informational:
                    WriteWarning($"[INFO] {change.Message}");
                    break;
                default:
                    break;
            }
        }

        private static void WriteSuccess(string message)
        {
            var previousColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ForegroundColor = previousColor;
        }

        private static void WriteWarning(string message)
        {
            var previousColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ForegroundColor = previousColor;
        }

        private static void WriteInfo(string message)
        {
            var previousColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(message);
            Console.ForegroundColor = previousColor;
        }
    }
}