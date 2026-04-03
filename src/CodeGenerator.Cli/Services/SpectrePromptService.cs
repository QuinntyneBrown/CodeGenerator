// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Cli.Validation;
using Spectre.Console;

namespace CodeGenerator.Cli.Services;

public class SpectrePromptService : IInteractivePromptService
{
    public bool IsInteractive => !Console.IsInputRedirected;

    public GenerationOptions PromptForMissingOptions(GenerationOptions partial)
    {
        var name = string.IsNullOrWhiteSpace(partial.Name)
            ? AnsiConsole.Prompt(
                new TextPrompt<string>("Solution [green]name[/]:")
                    .Validate(n => !string.IsNullOrWhiteSpace(n)
                        ? Spectre.Console.ValidationResult.Success()
                        : Spectre.Console.ValidationResult.Error("Name is required")))
            : partial.Name;

        var output = AnsiConsole.Prompt(
            new TextPrompt<string>("Output [green]directory[/]:")
                .DefaultValue(partial.OutputDirectory));

        var framework = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Target [green]framework[/]:")
                .AddChoices("net9.0", "net8.0"));

        var slnx = AnsiConsole.Confirm("Use [green].slnx[/] solution format?", partial.Slnx);

        var localSource = AnsiConsole.Prompt(
            new TextPrompt<string>("Local source root [grey](optional, press Enter to skip)[/]:")
                .AllowEmpty());

        return new GenerationOptions
        {
            Name = name,
            OutputDirectory = output,
            Framework = framework,
            Slnx = slnx,
            LocalSourceRoot = string.IsNullOrWhiteSpace(localSource) ? null : localSource,
        };
    }

    public string? PromptForConfigFile(string directory, IReadOnlyList<string> candidates)
    {
        if (candidates.Count == 0)
        {
            return null;
        }

        var choices = new List<string>(candidates) { "(none)" };

        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"Multiple config files found in [green]{directory}[/]. Select one:")
                .AddChoices(choices));

        return selected == "(none)" ? null : selected;
    }
}
