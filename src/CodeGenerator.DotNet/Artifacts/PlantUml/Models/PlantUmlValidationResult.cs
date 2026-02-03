// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeGenerator.DotNet.Artifacts.PlantUml.Models;

/// <summary>
/// Represents the overall validation result for a PlantUML solution architecture.
/// </summary>
public class PlantUmlValidationResult
{
    public PlantUmlValidationResult()
    {
        DocumentResults = [];
        GlobalIssues = [];
    }

    /// <summary>
    /// Gets or sets the source directory path that was validated.
    /// </summary>
    public string SourcePath { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when validation was performed.
    /// </summary>
    public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the validation results for each document.
    /// </summary>
    public List<PlantUmlDocumentValidationResult> DocumentResults { get; set; }

    /// <summary>
    /// Gets or sets issues that span multiple documents or are solution-wide.
    /// </summary>
    public List<PlantUmlValidationIssue> GlobalIssues { get; set; }

    /// <summary>
    /// Gets whether all validation passed with no errors.
    /// </summary>
    public bool IsValid => !HasErrors;

    /// <summary>
    /// Gets whether there are any error-level issues.
    /// </summary>
    public bool HasErrors => GlobalIssues.Any(i => i.Severity == ValidationSeverity.Error) ||
                            DocumentResults.Any(d => d.HasErrors);

    /// <summary>
    /// Gets whether there are any warning-level issues.
    /// </summary>
    public bool HasWarnings => GlobalIssues.Any(i => i.Severity == ValidationSeverity.Warning) ||
                              DocumentResults.Any(d => d.HasWarnings);

    /// <summary>
    /// Gets the total number of errors across all documents.
    /// </summary>
    public int TotalErrors => GlobalIssues.Count(i => i.Severity == ValidationSeverity.Error) +
                             DocumentResults.Sum(d => d.ErrorCount);

    /// <summary>
    /// Gets the total number of warnings across all documents.
    /// </summary>
    public int TotalWarnings => GlobalIssues.Count(i => i.Severity == ValidationSeverity.Warning) +
                               DocumentResults.Sum(d => d.WarningCount);

    /// <summary>
    /// Gets the total number of info-level issues across all documents.
    /// </summary>
    public int TotalInfo => GlobalIssues.Count(i => i.Severity == ValidationSeverity.Info) +
                           DocumentResults.Sum(d => d.InfoCount);

    /// <summary>
    /// Generates a detailed markdown report of the validation results.
    /// </summary>
    public string ToMarkdownReport()
    {
        var sb = new StringBuilder();

        sb.AppendLine("# PlantUML Solution Architecture Validation Report");
        sb.AppendLine();
        sb.AppendLine("## Summary");
        sb.AppendLine();
        sb.AppendLine($"- **Source Path:** `{SourcePath}`");
        sb.AppendLine($"- **Validated At:** {ValidatedAt:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine($"- **Overall Status:** {(IsValid ? "PASSED" : "FAILED")}");
        sb.AppendLine($"- **Documents Analyzed:** {DocumentResults.Count}");
        sb.AppendLine();

        // Statistics table
        sb.AppendLine("### Issue Statistics");
        sb.AppendLine();
        sb.AppendLine("| Severity | Count |");
        sb.AppendLine("|----------|-------|");
        sb.AppendLine($"| Errors | {TotalErrors} |");
        sb.AppendLine($"| Warnings | {TotalWarnings} |");
        sb.AppendLine($"| Info | {TotalInfo} |");
        sb.AppendLine();

        // Global issues section
        if (GlobalIssues.Count > 0)
        {
            sb.AppendLine("---");
            sb.AppendLine();
            sb.AppendLine("## Global Issues");
            sb.AppendLine();
            sb.AppendLine("Issues that affect the entire solution or span multiple documents.");
            sb.AppendLine();

            var errorIssues = GlobalIssues.Where(i => i.Severity == ValidationSeverity.Error).ToList();
            var warningIssues = GlobalIssues.Where(i => i.Severity == ValidationSeverity.Warning).ToList();
            var infoIssues = GlobalIssues.Where(i => i.Severity == ValidationSeverity.Info).ToList();

            if (errorIssues.Count > 0)
            {
                sb.AppendLine("### Errors");
                sb.AppendLine();
                foreach (var issue in errorIssues)
                {
                    sb.AppendLine(issue.ToMarkdown());
                }

                sb.AppendLine();
            }

            if (warningIssues.Count > 0)
            {
                sb.AppendLine("### Warnings");
                sb.AppendLine();
                foreach (var issue in warningIssues)
                {
                    sb.AppendLine(issue.ToMarkdown());
                }

                sb.AppendLine();
            }

            if (infoIssues.Count > 0)
            {
                sb.AppendLine("### Information");
                sb.AppendLine();
                foreach (var issue in infoIssues)
                {
                    sb.AppendLine(issue.ToMarkdown());
                }

                sb.AppendLine();
            }
        }

        // Per-document results
        if (DocumentResults.Count > 0)
        {
            sb.AppendLine("---");
            sb.AppendLine();
            sb.AppendLine("## Document Validation Results");
            sb.AppendLine();

            foreach (var docResult in DocumentResults.OrderByDescending(d => d.ErrorCount).ThenByDescending(d => d.WarningCount))
            {
                sb.AppendLine(docResult.ToMarkdown());
                sb.AppendLine();
            }
        }

        // How to fix section
        if (HasErrors || HasWarnings)
        {
            sb.AppendLine("---");
            sb.AppendLine();
            sb.AppendLine("## How to Fix Issues");
            sb.AppendLine();
            sb.AppendLine("Please review the issues listed above and make the necessary corrections to your PlantUML files.");
            sb.AppendLine("Once fixed, re-run the validation command to ensure all issues are resolved.");
            sb.AppendLine();
            sb.AppendLine("For more information on the PlantUML scaffolding specification, refer to:");
            sb.AppendLine("`docs/specs/plantuml-scaffolding.spec`");
            sb.AppendLine();
        }
        else
        {
            sb.AppendLine("---");
            sb.AppendLine();
            sb.AppendLine("## Next Steps");
            sb.AppendLine();
            sb.AppendLine("All validation checks passed! You can now generate a solution from these PlantUML files using:");
            sb.AppendLine();
            sb.AppendLine("```bash");
            sb.AppendLine("endpoint solution-create-from-plant-uml -n <SolutionName> -p <PlantUmlSourcePath>");
            sb.AppendLine("```");
            sb.AppendLine();
        }

        return sb.ToString();
    }
}

/// <summary>
/// Represents validation results for a single PlantUML document.
/// </summary>
public class PlantUmlDocumentValidationResult
{
    public PlantUmlDocumentValidationResult()
    {
        Issues = [];
        EntityResults = [];
    }

    /// <summary>
    /// Gets or sets the file path of the document.
    /// </summary>
    public string FilePath { get; set; }

    /// <summary>
    /// Gets or sets the document title.
    /// </summary>
    public string DocumentTitle { get; set; }

    /// <summary>
    /// Gets or sets the list of validation issues for this document.
    /// </summary>
    public List<PlantUmlValidationIssue> Issues { get; set; }

    /// <summary>
    /// Gets or sets detailed validation results for each entity in the document.
    /// </summary>
    public List<PlantUmlEntityValidationResult> EntityResults { get; set; }

    /// <summary>
    /// Gets whether the document has any errors.
    /// </summary>
    public bool HasErrors => Issues.Any(i => i.Severity == ValidationSeverity.Error) ||
                            EntityResults.Any(e => e.HasErrors);

    /// <summary>
    /// Gets whether the document has any warnings.
    /// </summary>
    public bool HasWarnings => Issues.Any(i => i.Severity == ValidationSeverity.Warning) ||
                              EntityResults.Any(e => e.HasWarnings);

    /// <summary>
    /// Gets the error count for this document.
    /// </summary>
    public int ErrorCount => Issues.Count(i => i.Severity == ValidationSeverity.Error) +
                            EntityResults.Sum(e => e.ErrorCount);

    /// <summary>
    /// Gets the warning count for this document.
    /// </summary>
    public int WarningCount => Issues.Count(i => i.Severity == ValidationSeverity.Warning) +
                              EntityResults.Sum(e => e.WarningCount);

    /// <summary>
    /// Gets the info count for this document.
    /// </summary>
    public int InfoCount => Issues.Count(i => i.Severity == ValidationSeverity.Info) +
                           EntityResults.Sum(e => e.InfoCount);

    public string ToMarkdown()
    {
        var sb = new StringBuilder();

        var statusIcon = !HasErrors && !HasWarnings ? "[PASS]" : (HasErrors ? "[FAIL]" : "[WARN]");
        var fileName = System.IO.Path.GetFileName(FilePath);

        sb.AppendLine($"### {statusIcon} `{fileName}`");
        sb.AppendLine();
        sb.AppendLine($"**Path:** `{FilePath}`");

        if (!string.IsNullOrEmpty(DocumentTitle))
        {
            sb.AppendLine($"**Title:** {DocumentTitle}");
        }

        sb.AppendLine();

        if (!HasErrors && !HasWarnings && Issues.Count == 0 && EntityResults.All(e => e.Issues.Count == 0))
        {
            sb.AppendLine("No issues found.");
            return sb.ToString();
        }

        // Document-level issues
        var docErrors = Issues.Where(i => i.Severity == ValidationSeverity.Error).ToList();
        var docWarnings = Issues.Where(i => i.Severity == ValidationSeverity.Warning).ToList();
        var docInfo = Issues.Where(i => i.Severity == ValidationSeverity.Info).ToList();

        if (docErrors.Count > 0)
        {
            sb.AppendLine("#### Document Errors");
            sb.AppendLine();
            foreach (var issue in docErrors)
            {
                sb.AppendLine(issue.ToMarkdown());
            }

            sb.AppendLine();
        }

        if (docWarnings.Count > 0)
        {
            sb.AppendLine("#### Document Warnings");
            sb.AppendLine();
            foreach (var issue in docWarnings)
            {
                sb.AppendLine(issue.ToMarkdown());
            }

            sb.AppendLine();
        }

        if (docInfo.Count > 0)
        {
            sb.AppendLine("#### Document Information");
            sb.AppendLine();
            foreach (var issue in docInfo)
            {
                sb.AppendLine(issue.ToMarkdown());
            }

            sb.AppendLine();
        }

        // Entity-level issues
        var entitiesWithIssues = EntityResults.Where(e => e.Issues.Count > 0).ToList();
        if (entitiesWithIssues.Count > 0)
        {
            sb.AppendLine("#### Entity Issues");
            sb.AppendLine();
            foreach (var entity in entitiesWithIssues)
            {
                sb.AppendLine(entity.ToMarkdown());
            }
        }

        return sb.ToString();
    }
}

/// <summary>
/// Represents validation results for a specific entity (class/enum).
/// </summary>
public class PlantUmlEntityValidationResult
{
    public PlantUmlEntityValidationResult()
    {
        Issues = [];
    }

    /// <summary>
    /// Gets or sets the entity name.
    /// </summary>
    public string EntityName { get; set; }

    /// <summary>
    /// Gets or sets the entity type (class, enum, etc.).
    /// </summary>
    public string EntityType { get; set; }

    /// <summary>
    /// Gets or sets the namespace/package of the entity.
    /// </summary>
    public string Namespace { get; set; }

    /// <summary>
    /// Gets or sets the list of validation issues for this entity.
    /// </summary>
    public List<PlantUmlValidationIssue> Issues { get; set; }

    /// <summary>
    /// Gets whether the entity has any errors.
    /// </summary>
    public bool HasErrors => Issues.Any(i => i.Severity == ValidationSeverity.Error);

    /// <summary>
    /// Gets whether the entity has any warnings.
    /// </summary>
    public bool HasWarnings => Issues.Any(i => i.Severity == ValidationSeverity.Warning);

    /// <summary>
    /// Gets the error count for this entity.
    /// </summary>
    public int ErrorCount => Issues.Count(i => i.Severity == ValidationSeverity.Error);

    /// <summary>
    /// Gets the warning count for this entity.
    /// </summary>
    public int WarningCount => Issues.Count(i => i.Severity == ValidationSeverity.Warning);

    /// <summary>
    /// Gets the info count for this entity.
    /// </summary>
    public int InfoCount => Issues.Count(i => i.Severity == ValidationSeverity.Info);

    public string ToMarkdown()
    {
        var sb = new StringBuilder();

        sb.AppendLine($"**{EntityType}: `{EntityName}`**");

        if (!string.IsNullOrEmpty(Namespace))
        {
            sb.AppendLine($"  - Namespace: `{Namespace}`");
        }

        sb.AppendLine();

        foreach (var issue in Issues.OrderBy(i => i.Severity))
        {
            sb.AppendLine($"  {issue.ToMarkdown()}");
        }

        return sb.ToString();
    }
}

/// <summary>
/// Represents a single validation issue.
/// </summary>
public class PlantUmlValidationIssue
{
    /// <summary>
    /// Gets or sets the issue code for categorization.
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Gets or sets the severity of the issue.
    /// </summary>
    public ValidationSeverity Severity { get; set; }

    /// <summary>
    /// Gets or sets the category of the issue.
    /// </summary>
    public ValidationCategory Category { get; set; }

    /// <summary>
    /// Gets or sets the issue message.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets additional details or context for the issue.
    /// </summary>
    public string Details { get; set; }

    /// <summary>
    /// Gets or sets the suggested fix for the issue.
    /// </summary>
    public string SuggestedFix { get; set; }

    /// <summary>
    /// Gets or sets the line number where the issue was found (if applicable).
    /// </summary>
    public int? LineNumber { get; set; }

    /// <summary>
    /// Gets or sets the related element name (class, property, etc.).
    /// </summary>
    public string RelatedElement { get; set; }

    public string ToMarkdown()
    {
        var severityIcon = Severity switch
        {
            ValidationSeverity.Error => "ERROR",
            ValidationSeverity.Warning => "WARNING",
            ValidationSeverity.Info => "INFO",
            _ => "?"
        };

        var sb = new StringBuilder();
        sb.Append($"- **[{severityIcon}]** `{Code}`: {Message}");

        if (!string.IsNullOrEmpty(RelatedElement))
        {
            sb.Append($" (Element: `{RelatedElement}`)");
        }

        if (LineNumber.HasValue)
        {
            sb.Append($" [Line {LineNumber}]");
        }

        if (!string.IsNullOrEmpty(Details))
        {
            sb.AppendLine();
            sb.Append($"  - Details: {Details}");
        }

        if (!string.IsNullOrEmpty(SuggestedFix))
        {
            sb.AppendLine();
            sb.Append($"  - Fix: {SuggestedFix}");
        }

        return sb.ToString();
    }
}

/// <summary>
/// Validation severity levels.
/// </summary>
public enum ValidationSeverity
{
    /// <summary>
    /// Informational message - does not prevent solution generation.
    /// </summary>
    Info,

    /// <summary>
    /// Warning - may cause issues but does not prevent solution generation.
    /// </summary>
    Warning,

    /// <summary>
    /// Error - prevents successful solution generation.
    /// </summary>
    Error
}

/// <summary>
/// Categories for validation issues.
/// </summary>
public enum ValidationCategory
{
    /// <summary>
    /// Document structure issues (missing @startuml/@enduml, etc.).
    /// </summary>
    DocumentStructure,

    /// <summary>
    /// Missing required documents.
    /// </summary>
    MissingDocument,

    /// <summary>
    /// Entity definition issues.
    /// </summary>
    EntityDefinition,

    /// <summary>
    /// Property definition issues.
    /// </summary>
    PropertyDefinition,

    /// <summary>
    /// Type reference issues.
    /// </summary>
    TypeReference,

    /// <summary>
    /// Relationship definition issues.
    /// </summary>
    Relationship,

    /// <summary>
    /// Namespace/package naming issues.
    /// </summary>
    Naming,

    /// <summary>
    /// Stereotype issues.
    /// </summary>
    Stereotype,

    /// <summary>
    /// Duplicate definitions.
    /// </summary>
    Duplicate,

    /// <summary>
    /// Consistency issues between documents.
    /// </summary>
    Consistency,

    /// <summary>
    /// Best practice recommendations.
    /// </summary>
    BestPractice
}
