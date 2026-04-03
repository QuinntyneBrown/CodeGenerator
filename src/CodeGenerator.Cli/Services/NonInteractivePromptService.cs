// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Cli.Validation;

namespace CodeGenerator.Cli.Services;

public class NonInteractivePromptService : IInteractivePromptService
{
    public bool IsInteractive => false;

    public GenerationOptions PromptForMissingOptions(GenerationOptions partial)
    {
        if (string.IsNullOrWhiteSpace(partial.Name))
        {
            throw new InvalidOperationException(
                "Required option '--name' was not provided and interactive mode is not available "
                + "(stdin is not a terminal). Provide all required options on the command line.");
        }

        return partial;
    }
}
