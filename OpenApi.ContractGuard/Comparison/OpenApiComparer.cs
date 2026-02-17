using Microsoft.OpenApi.Models;
using OpenApi.ContractGuard.Comparison.enums;
using System;
using System.Collections.Generic;
using System.Text;



namespace OpenApi.ContractGuard.Comparison
{
    public class OpenApiComparer : IContractComparer
    {
        public List<ContractChange> Compare(
            OpenApiDocument oldDoc,
            OpenApiDocument newDoc)
        {
            var changes = new List<ContractChange>();

            CheckPaths(oldDoc, newDoc, changes);
            CheckOperations(oldDoc, newDoc, changes);
            CheckVersions(oldDoc, newDoc, changes);

            return changes;
        }

        private void CheckOperations(
         OpenApiDocument oldDoc,
         OpenApiDocument newDoc,
         List<ContractChange> changes)
        {
            foreach (var path in oldDoc.Paths)
            {
                if (!newDoc.Paths.ContainsKey(path.Key))
                    continue;

                var oldOps = path.Value.Operations;
                var newOps = newDoc.Paths[path.Key].Operations;

                // Added operations
                foreach (var op in newOps)
                {
                    if (!oldOps.ContainsKey(op.Key))
                    {
                        changes.Add(ContractChange.Create(
                            ChangeType.OperationAdded,
                            $"{op.Key} added on {path.Key}",
                            path.Key,
                            op.Key.ToString()));
                    }
                }

                // Removed operations
                foreach (var op in oldOps)
                {
                    if (!newOps.ContainsKey(op.Key))
                    {
                        changes.Add(ContractChange.Create(
                            ChangeType.OperationRemoved,
                            $"{op.Key} removed from {path.Key}",
                            path.Key,
                            op.Key.ToString()));
                    }
                }
            }
        }

        private void CheckVersions(
            OpenApiDocument oldDoc,
            OpenApiDocument newDoc,
            List<ContractChange> changes)
        {
            var oldVersion = oldDoc.Info.Version;
            var newVersion = newDoc.Info.Version;
            if (!String.Equals(oldVersion, newVersion, StringComparison.Ordinal))
            {
                changes.Add(ContractChange.Create(
                    ChangeType.VersionChanged,
                    $"API version changed from {oldVersion} to {newVersion}"));
            }
        }

        private void CheckPaths(
                    OpenApiDocument oldDoc,
                    OpenApiDocument newDoc,
                    List<ContractChange> changes)
        {
            foreach (var newPath in newDoc.Paths)
            {
                if (!oldDoc.Paths.ContainsKey(newPath.Key))
                {
                    changes.Add(ContractChange.Create(
                        ChangeType.PathAdded,
                        $"Path added: {newPath.Key}",
                        newPath.Key));
                }
            }

            foreach (var oldPath in oldDoc.Paths)
            {
                {
                    if (!newDoc.Paths.ContainsKey(oldPath.Key))
                        changes.Add(ContractChange.Create(
                        ChangeType.PathRemoved,
                        $"Path removed: {oldPath.Key}",
                        oldPath.Key));
                }
            }
        }

    }
}
