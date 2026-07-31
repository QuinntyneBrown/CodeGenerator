// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Cli.Validation;
using CodeGenerator.Core.Errors;
using CodeGenerator.Core.Validation;

namespace CodeGenerator.Cli.Services;

public class NonInteractivePromptService : IInteractivePromptService
{
    public bool IsInteractive => false;

    public GenerationOptions PromptForMissingOptions(GenerationOptions partial)
    {
        if (string.IsNullOrWhiteSpace(partial.Name))
        {
            // A plain exception here would reach the catch-all in Program and exit 99
            // as "ERROR [INTERNAL]", discarding the guidance below. A CliValidationException
            // carries exit code 1 and prints the message.
            var result = new ValidationResult();
            result.AddError(
                nameof(GenerationOptions.Name),
                "Required option '--name' was not provided and interactive mode is not available "
                + "(stdin is not a terminal). Provide all required options on the command line.");

            throw new CliValidationException(result);
        }

        return partial;
    }

    public string? PromptForConfigFile(string directory, IReadOnlyList<string> candidates)
    {
        var result = new ValidationResult();
        result.AddError(
            "--config",
            $"Multiple config files found in '{directory}' and interactive mode is not available "
            + "(stdin is not a terminal). Specify the config file explicitly via --config.");

        throw new CliValidationException(result);
    }
}
