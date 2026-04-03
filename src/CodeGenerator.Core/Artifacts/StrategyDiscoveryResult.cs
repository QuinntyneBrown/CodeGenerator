// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;

namespace CodeGenerator.Core.Artifacts;

public record StrategyLoadError(
    string TypeName,
    string AssemblyName,
    string ErrorMessage);

public record StrategyDiscoveryResult(
    int TotalTypesScanned,
    int StrategiesRegistered,
    int FailedToLoad,
    IReadOnlyList<StrategyLoadError> LoadErrors)
{
    public bool HasErrors => LoadErrors.Count > 0;

    public string ToSummary()
    {
        var sb = new StringBuilder();
        sb.Append($"Scanned {TotalTypesScanned} types: {StrategiesRegistered} registered, {FailedToLoad} failed.");

        if (HasErrors)
        {
            sb.AppendLine();
            sb.AppendLine("Errors:");

            foreach (var error in LoadErrors)
            {
                sb.AppendLine($"  - {error.TypeName} ({error.AssemblyName}): {error.ErrorMessage}");
            }
        }

        return sb.ToString();
    }
}
