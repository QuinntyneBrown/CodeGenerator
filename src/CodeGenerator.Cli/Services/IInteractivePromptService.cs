// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Cli.Validation;

namespace CodeGenerator.Cli.Services;

public interface IInteractivePromptService
{
    bool IsInteractive { get; }

    GenerationOptions PromptForMissingOptions(GenerationOptions partial);

    string? PromptForConfigFile(string directory, IReadOnlyList<string> candidates);
}
