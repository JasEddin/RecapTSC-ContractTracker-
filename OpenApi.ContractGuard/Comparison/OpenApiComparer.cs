using Microsoft.OpenApi.Models;
using OpenApi.ContractGuard.Comparison.enums;
using System;
using System.Collections.Generic;
using System.Text;



namespace OpenApi.ContractGuard.Comparison
{
    public class OpenApiComparer
    {
        public List<ContractChange> Compare(
            OpenApiDocument oldDoc,
            OpenApiDocument newDoc)
        {
            var changes = new List<ContractChange>();

             CheckPaths(oldDoc, newDoc, changes);

            return changes;
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
                if (!newDoc.Paths.ContainsKey(oldPath.Key))
                {
                    changes.Add(ContractChange.Create(
                        ChangeType.PathRemoved,
                        $"Path removed: {oldPath.Key}",
                        oldPath.Key));
                }
            }
        }

    }
}
