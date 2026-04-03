// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Validation;

public class ValidationResult
{
    public bool IsValid => Errors.Count == 0;

    public List<ValidationError> Errors { get; } = [];

    public List<ValidationError> Warnings { get; } = [];

    public List<ValidationError> InfoMessages { get; } = [];

    public IReadOnlyList<ValidationError> All =>
        [.. Errors, .. Warnings, .. InfoMessages];

    public void AddError(string propertyName, string message)
    {
        Errors.Add(new ValidationError
        {
            PropertyName = propertyName,
            ErrorMessage = message,
            Severity = ValidationSeverity.Error,
        });
    }

    public void AddWarning(string propertyName, string message)
    {
        Warnings.Add(new ValidationError
        {
            PropertyName = propertyName,
            ErrorMessage = message,
            Severity = ValidationSeverity.Warning,
        });
    }

    public void AddInfo(string propertyName, string message)
    {
        InfoMessages.Add(new ValidationError
        {
            PropertyName = propertyName,
            ErrorMessage = message,
            Severity = ValidationSeverity.Info,
        });
    }

    public void Merge(ValidationResult other)
    {
        Errors.AddRange(other.Errors);
        Warnings.AddRange(other.Warnings);
        InfoMessages.AddRange(other.InfoMessages);
    }

    public ValidationResult WithContext(string contextName)
    {
        var result = new ValidationResult();

        foreach (var error in Errors)
        {
            result.Errors.Add(new ValidationError
            {
                PropertyName = $"{contextName}.{error.PropertyName}",
                ErrorMessage = error.ErrorMessage,
                Severity = error.Severity,
            });
        }

        foreach (var warning in Warnings)
        {
            result.Warnings.Add(new ValidationError
            {
                PropertyName = $"{contextName}.{warning.PropertyName}",
                ErrorMessage = warning.ErrorMessage,
                Severity = warning.Severity,
            });
        }

        foreach (var info in InfoMessages)
        {
            result.InfoMessages.Add(new ValidationError
            {
                PropertyName = $"{contextName}.{info.PropertyName}",
                ErrorMessage = info.ErrorMessage,
                Severity = info.Severity,
            });
        }

        return result;
    }

    public string ToFormattedString(bool includeWarnings = true)
    {
        var lines = new List<string>();

        foreach (var error in Errors)
        {
            lines.Add($"ERROR [{error.PropertyName}]: {error.ErrorMessage}");
        }

        if (includeWarnings)
        {
            foreach (var warning in Warnings)
            {
                lines.Add($"WARNING [{warning.PropertyName}]: {warning.ErrorMessage}");
            }
        }

        foreach (var info in InfoMessages)
        {
            lines.Add($"INFO [{info.PropertyName}]: {info.ErrorMessage}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    public Dictionary<string, List<ValidationError>> GroupByProperty()
    {
        var grouped = new Dictionary<string, List<ValidationError>>();

        foreach (var entry in All)
        {
            if (!grouped.TryGetValue(entry.PropertyName, out var list))
            {
                list = [];
                grouped[entry.PropertyName] = list;
            }

            list.Add(entry);
        }

        return grouped;
    }

    public static ValidationResult Success() => new();
}
