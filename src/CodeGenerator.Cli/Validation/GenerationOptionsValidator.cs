// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;
using CodeGenerator.Core.Validation;

namespace CodeGenerator.Cli.Validation;

public class GenerationOptionsValidator
{
    private readonly Validator<GenerationOptions> _validator;
    private readonly FileSystemRules _fsRules;

    public GenerationOptionsValidator(IFileSystem fileSystem)
    {
        _fsRules = new FileSystemRules(fileSystem);

        _validator = new Validator<GenerationOptions>()
            .RuleFor(x => x.Name, CommonRules.IsNotEmpty, "Solution name is required.")
            .RuleFor(x => x.Name, CommonRules.IsValidCSharpIdentifier, "Solution name is not a valid C# identifier. Must start with a letter or underscore, followed by letters, digits, underscores, or dots.")
            .RuleFor(x => x.Framework, CommonRules.IsNotEmpty, "Target framework is required.")
            .RuleFor(x => x.Framework, CommonRules.IsSupportedFrameworkVersion, "Invalid target framework. Must start with 'net' (e.g., 'net8.0', 'net9.0').")
            .RuleFor(x => x.OutputDirectory, v => _fsRules.ParentDirectoryExists(v), "Parent directory does not exist.");
    }

    public ValidationResult Validate(GenerationOptions options)
    {
        return _validator.Validate(options);
    }
}
