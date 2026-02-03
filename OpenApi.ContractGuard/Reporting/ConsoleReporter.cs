using OpenApi.ContractGuard.Comparison;
using OpenApi.ContractGuard.Comparison.enums;

namespace OpenApi.ContractGuard.Reporting
{
    public static class ConsoleReporter
    {
        public static void ReportChanges(IEnumerable<ContractChange> changes)
        {
            if (!changes.Any())
            {
                WriteSuccess("No contract changes detected.");
                return;
            }

            WriteWarning("Detected Changes:");

            if (changes.Any(change => change.Impact == ChangeImpact.ContractUpdateRequired))
            {
                // I want to make this more visible

                WriteError("Contract changes detected that require an update to the contract.");
            }

            foreach (var change in changes)
            {
                WriteChange(change);
            }
        }

        private static void WriteChange(ContractChange change)
        {
            switch (change.Impact)
            {
                case ChangeImpact.ContractUpdateRequired:
                    WriteError($"{change.Message}");
                    break;
                case ChangeImpact.Informational:
                    WriteWarning($"{change.Message}");
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

        private static void WriteError(string message)
        {
            var previousColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
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
    }
}




