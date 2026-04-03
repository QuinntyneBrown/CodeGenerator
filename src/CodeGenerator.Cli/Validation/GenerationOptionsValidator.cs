// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;
using System.Text.RegularExpressions;
using CodeGenerator.Core.Validation;

namespace CodeGenerator.Cli.Validation;

public class GenerationOptionsValidator
{
    private static readonly Regex ValidCSharpIdentifier = new(
        @"^[A-Za-z_][A-Za-z0-9_.]*$",
        RegexOptions.Compiled);

    private readonly IFileSystem _fileSystem;

    public GenerationOptionsValidator(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public ValidationResult Validate(GenerationOptions options)
    {
        var result = new ValidationResult();

        ValidateName(options.Name, result);
        ValidateOutputDirectory(options.OutputDirectory, result);
        ValidateFramework(options.Framework, result);

        return result;
    }

    private void ValidateName(string name, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            result.AddError(nameof(GenerationOptions.Name),
                "Solution name is required.");
            return;
        }

        if (!ValidCSharpIdentifier.IsMatch(name))
        {
            result.AddError(nameof(GenerationOptions.Name),
                $"'{name}' is not a valid C# identifier. " +
                "Must start with a letter or underscore, followed by letters, digits, underscores, or dots.");
        }

        if (name.Length > 128)
        {
            result.AddWarning(nameof(GenerationOptions.Name),
                "Solution name exceeds 128 characters. This may cause issues on some file systems.");
        }
    }

    private void ValidateOutputDirectory(string outputDirectory, ValidationResult result)
    {
        var parentDir = _fileSystem.Path.GetDirectoryName(outputDirectory);

        if (!string.IsNullOrEmpty(parentDir) && !_fileSystem.Directory.Exists(parentDir))
        {
            result.AddError(nameof(GenerationOptions.OutputDirectory),
                $"Parent directory does not exist: '{parentDir}'");
            return;
        }

        if (_fileSystem.Directory.Exists(outputDirectory))
        {
            if (!IsDirectoryWritable(outputDirectory))
            {
                result.AddError(nameof(GenerationOptions.OutputDirectory),
                    $"Output directory is not writable: '{outputDirectory}'");
            }
        }
    }

    private void ValidateFramework(string framework, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(framework))
        {
            result.AddError(nameof(GenerationOptions.Framework),
                "Target framework is required.");
            return;
        }

        if (!framework.StartsWith("net", StringComparison.OrdinalIgnoreCase))
        {
            result.AddError(nameof(GenerationOptions.Framework),
                $"'{framework}' is not a valid target framework. " +
                "Must start with 'net' (e.g., 'net8.0', 'net9.0').");
        }
    }

    private bool IsDirectoryWritable(string path)
    {
        try
        {
            var testFile = _fileSystem.Path.Combine(path, $".codegen-write-test-{Guid.NewGuid():N}");
            using var stream = _fileSystem.File.Create(testFile);
            stream.Close();
            _fileSystem.File.Delete(testFile);
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
    }
}
