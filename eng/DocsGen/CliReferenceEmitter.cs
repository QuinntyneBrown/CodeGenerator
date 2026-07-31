// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Reflection;
using System.Text;
using CodeGenerator.Cli.Commands;
using CodeGenerator.Cli.Configuration;
using CodeGenerator.Core.Errors;

namespace CodeGenerator.DocsGen;

/// <summary>
/// Emits the CLI reference, the configuration tables, and the exit and error code
/// references. Every table is projected from a live object or a reflected type, never
/// from a hand-maintained list.
/// </summary>
public sealed class CliReferenceEmitter(string docsRoot, KnownLimitations limitations)
{
    public IEnumerable<string> Emit()
    {
        var root = CommandTreeWalker.Describe(CommandTreeWalker.BuildRoot());

        foreach (var written in EmitCommandIndex(root)) yield return written;
        foreach (var written in EmitCommandPages(root)) yield return written;
        foreach (var written in EmitGlobalOptions(root)) yield return written;
        foreach (var written in EmitExitCodes()) yield return written;
        foreach (var written in EmitErrorCodes()) yield return written;
        foreach (var written in EmitConfigurationTables()) yield return written;
    }

    // ---------------------------------------------------------------- CLI reference

    private IEnumerable<string> EmitCommandIndex(DocumentedCommand root)
    {
        var page = MarkdownEmitter.Page(
            "CLI reference",
            $"Every command and option of {CliOptions.ToolCommandName}, generated from the command tree.",
            order: 1);

        page.AppendLine($"`{CliOptions.ToolCommandName}` {Lowercase(root.Description)}");
        page.AppendLine();
        page.AppendLine("## Commands");
        page.AppendLine();
        page.AppendLine("| Command | Description |");
        page.AppendLine("|---|---|");
        page.AppendLine($"| [`{root.Name}`](/cli/{Slug(root.Name)}/) | {MarkdownEmitter.Cell(root.Description)} |");

        foreach (var child in root.Subcommands)
        {
            page.AppendLine(
                $"| [`{root.Name} {child.Name}`](/cli/{Slug(child.Name)}/) | {MarkdownEmitter.Cell(child.Description)} |");
        }

        page.AppendLine();
        page.AppendLine("Every command also accepts the [global options](/cli/global-options/).");
        page.AppendLine();
        page.AppendLine("## Exit codes");
        page.AppendLine();
        page.AppendLine(
            "Each command returns one of the documented [exit codes](/reference/exit-codes/). "
            + "A script can branch on the code without parsing output.");

        var path = Path.Combine(docsRoot, "cli", "index.md");
        return MarkdownEmitter.WriteIfChanged(path, page.ToString()) ? [path] : [];
    }

    private IEnumerable<string> EmitCommandPages(DocumentedCommand root)
    {
        var written = new List<string>();
        var order = 2;

        foreach (var command in Flatten(root))
        {
            var isRoot = command.Path == root.Path;
            var title = isRoot ? command.Name : $"{root.Name} {command.Name}";

            var page = MarkdownEmitter.Page(title, Summarize(command.Description), order++);

            page.AppendLine(command.Description);
            page.AppendLine();
            page.AppendLine("## Synopsis");
            page.AppendLine();
            page.AppendLine("```bash");
            page.AppendLine(Synopsis(root, command, isRoot));
            page.AppendLine("```");
            page.AppendLine();

            if (isRoot && command.Subcommands.Count > 0)
            {
                page.AppendLine("## Subcommands");
                page.AppendLine();
                page.AppendLine("| Command | Description |");
                page.AppendLine("|---|---|");
                foreach (var child in command.Subcommands)
                {
                    page.AppendLine(
                        $"| [`{child.Name}`](/cli/{Slug(child.Name)}/) | {MarkdownEmitter.Cell(child.Description)} |");
                }

                page.AppendLine();
            }

            AppendOptionsTable(page, command.Options);
            AppendOverride(page, command.Path);

            var path = Path.Combine(docsRoot, "cli", $"{Slug(command.Name)}.md");
            if (MarkdownEmitter.WriteIfChanged(path, page.ToString()))
            {
                written.Add(path);
            }
        }

        return written;
    }

    private void AppendOptionsTable(StringBuilder page, IReadOnlyList<DocumentedOption> options)
    {
        if (options.Count == 0)
        {
            return;
        }

        page.AppendLine("## Options");
        page.AppendLine();
        page.AppendLine("| Option | Type | Default | Required | Description |");
        page.AppendLine("|---|---|---|---|---|");

        foreach (var option in options)
        {
            var type = option.IsFlag ? "flag" : option.ValueType;
            var defaultValue = option.IsFlag && option.DefaultValue is null ? "false" : option.DefaultValue;

            page.AppendLine(
                $"| `{option.DisplayAliases}` "
                + $"| {type} "
                + $"| {MarkdownEmitter.Code(defaultValue)} "
                + $"| {(option.IsRequired ? "yes" : "no")} "
                + $"| {MarkdownEmitter.Cell(option.Description)}{LimitationSuffix(option.CanonicalAlias)} |");
        }

        page.AppendLine();

        foreach (var option in options)
        {
            var limitation = limitations.ForSurface(option.CanonicalAlias);
            if (limitation is not null)
            {
                page.AppendLine(limitations.RenderAside(limitation));
                page.AppendLine();
            }
        }
    }

    private string LimitationSuffix(string alias) =>
        limitations.ForSurface(alias) is { } limitation ? $" **See {limitation.Id}.**" : string.Empty;

    private IEnumerable<string> EmitGlobalOptions(DocumentedCommand root)
    {
        var globals = root.Options
            .Where(option => option.CanonicalAlias is "--verbose")
            .ToList();

        var page = MarkdownEmitter.Page(
            "Global options",
            "Options accepted by every command.",
            order: 90);

        page.AppendLine("These options are accepted by every command in the tree.");
        page.AppendLine();
        AppendOptionsTable(page, globals);

        page.AppendLine("`--help` and `--version` are supplied by the command-line host.");
        page.AppendLine();
        page.AppendLine("| Option | Description |");
        page.AppendLine("|---|---|");
        page.AppendLine("| `--help`, `-h`, `-?` | Show help for the command and exit. |");
        page.AppendLine("| `--version` | Show the tool version and exit. |");

        var path = Path.Combine(docsRoot, "cli", "global-options.md");
        return MarkdownEmitter.WriteIfChanged(path, page.ToString()) ? [path] : [];
    }

    // ------------------------------------------------------------------- Exit codes

    private IEnumerable<string> EmitExitCodes()
    {
        var page = MarkdownEmitter.Page(
            "Exit codes",
            "The process exit code returned for each class of failure.",
            order: 1);

        page.AppendLine(
            "Every invocation returns one of the codes below. The code identifies the class of "
            + "failure, so a script can branch on it without parsing output.");
        page.AppendLine();
        page.AppendLine("| Code | Name | Meaning | Raised as |");
        page.AppendLine("|---|---|---|---|");

        foreach (var (name, value) in Constants(typeof(CliExitCodes)).OrderBy(c => (int)c.Value!))
        {
            page.AppendLine(
                $"| `{value}` | `{name}` | {MarkdownEmitter.Cell(ExitCodeMeaning(name))} | {ExitCodeException(name)} |");
        }

        page.AppendLine();
        page.AppendLine("## Notes");
        page.AppendLine();
        page.AppendLine(
            "- `8` is returned when the run is interrupted with <kbd>Ctrl</kbd>+<kbd>C</kbd>. "
            + "Files already written are left in place.");
        page.AppendLine(
            "- `99` indicates a failure the tool did not anticipate. Re-run with `--verbose` "
            + "to see the stack trace, and please report it.");

        var path = Path.Combine(docsRoot, "reference", "exit-codes.md");
        return MarkdownEmitter.WriteIfChanged(path, page.ToString()) ? [path] : [];
    }

    private static string ExitCodeMeaning(string name) => name switch
    {
        "Success" => "The command completed.",
        "ValidationError" => "An option, argument, or configuration file failed validation.",
        "IoError" => "A file or directory could not be read or written.",
        "ProcessError" => "An external command returned a non-zero exit code.",
        "TemplateError" => "A template could not be found, parsed, or rendered.",
        "ConfigurationError" => "Configuration was missing or could not be understood.",
        "PluginError" => "A plugin failed to load, or a generation strategy threw.",
        "SchemaError" => "Input did not match its schema.",
        "Cancelled" => "The run was cancelled before it completed.",
        "UnexpectedError" => "An unanticipated failure. Re-run with `--verbose`.",
        _ => "—",
    };

    private static string ExitCodeException(string name) => name switch
    {
        "Success" => "—",
        "ValidationError" => "`CliValidationException`",
        "IoError" => "`CliIOException`",
        "ProcessError" => "`CliProcessException`",
        "TemplateError" => "`CliTemplateException`",
        "ConfigurationError" => "`CliConfigurationException`",
        "PluginError" => "`CliPluginException`",
        "SchemaError" => "`CliSchemaException`",
        "Cancelled" => "`CliCancelledException`, `OperationCanceledException`",
        "UnexpectedError" => "any other exception",
        _ => "—",
    };

    // ------------------------------------------------------------------ Error codes

    private IEnumerable<string> EmitErrorCodes()
    {
        var page = MarkdownEmitter.Page(
            "Error codes",
            "Stable identifiers attached to each reported failure.",
            order: 2);

        page.AppendLine(
            "Reported failures carry a stable code alongside the message. The code does not "
            + "change when the wording does, so it is safe to match on.");
        page.AppendLine();

        foreach (var group in ErrorCodeGroups())
        {
            page.AppendLine($"## {group.Key}");
            page.AppendLine();
            page.AppendLine("| Code | Constant |");
            page.AppendLine("|---|---|");

            foreach (var (name, value) in group.OrderBy(c => c.Name, StringComparer.Ordinal))
            {
                page.AppendLine($"| `{value}` | `{group.Key}.{name}` |");
            }

            page.AppendLine();
        }

        var path = Path.Combine(docsRoot, "reference", "error-codes.md");
        return MarkdownEmitter.WriteIfChanged(path, page.ToString()) ? [path] : [];
    }

    /// <summary>
    /// <c>ErrorCodes</c> nests its groups in static classes. A flat field scan finds only
    /// the single top-level constant and looks plausible, so the walk must recurse.
    /// </summary>
    private static IEnumerable<IGrouping<string, (string Name, string Value)>> ErrorCodeGroups()
    {
        var results = new List<(string Group, string Name, string Value)>();

        void Walk(Type type, string group)
        {
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static).Where(f => f.IsLiteral))
            {
                results.Add((group, field.Name, (string)field.GetRawConstantValue()!));
            }

            foreach (var nested in type.GetNestedTypes(BindingFlags.Public))
            {
                Walk(nested, nested.Name);
            }
        }

        Walk(typeof(ErrorCodes), "General");

        return results
            .GroupBy(r => r.Group, r => (r.Name, r.Value))
            .OrderBy(g => g.Key == "General" ? string.Empty : g.Key, StringComparer.Ordinal);
    }

    // ---------------------------------------------------------------- Configuration

    private IEnumerable<string> EmitConfigurationTables()
    {
        var written = new List<string>();

        var env = MarkdownEmitter.Page(
            "Environment variables",
            "Environment variables recognized by the tool and the settings they supply.",
            order: 3);

        env.AppendLine(
            "Environment variables form the third configuration tier: they override the "
            + "configuration file and are overridden by command-line arguments. See "
            + "[precedence](/config/precedence/).");
        env.AppendLine();
        env.AppendLine("| Variable | Sets | Notes |");
        env.AppendLine("|---|---|---|");

        foreach (var (variable, key) in EnvironmentVariableMapper.KeyMap.OrderBy(p => p.Key, StringComparer.Ordinal))
        {
            env.AppendLine($"| `{variable}` | `{key}` | {MarkdownEmitter.Cell(SettingNote(key))} |");
        }

        env.AppendLine();
        env.AppendLine("A variable that is not set contributes nothing and leaves lower tiers standing.");

        var envPath = Path.Combine(docsRoot, "config", "environment.md");
        if (MarkdownEmitter.WriteIfChanged(envPath, env.ToString())) written.Add(envPath);

        var defaults = MarkdownEmitter.Page(
            "Built-in defaults",
            "The values used when no other configuration tier supplies one.",
            order: 4);

        defaults.AppendLine("These values apply when no configuration file, environment variable, or argument sets them.");
        defaults.AppendLine();
        defaults.AppendLine("| Setting | Default |");
        defaults.AppendLine("|---|---|");

        foreach (var (key, value) in ConfigBootstrap.GetBuiltInDefaults().OrderBy(p => p.Key, StringComparer.Ordinal))
        {
            defaults.AppendLine($"| `{key}` | `{value}` |");
        }

        defaults.AppendLine();
        defaults.AppendLine(
            "`output` has no built-in default. Each command falls back to the current working "
            + "directory, so `--output` may be omitted.");

        var defaultsPath = Path.Combine(docsRoot, "config", "defaults.md");
        if (MarkdownEmitter.WriteIfChanged(defaultsPath, defaults.ToString())) written.Add(defaultsPath);

        return written;
    }

    private static string SettingNote(string key) => key switch
    {
        "framework" => "Target framework moniker, for example `net9.0`.",
        "output" => "Output directory.",
        "slnx" => "`true` selects the XML-based `.slnx` solution format.",
        "templates.author" => "Author value available to templates.",
        "templates.license" => "License value available to templates.",
        _ => "—",
    };

    // ---------------------------------------------------------------------- Helpers

    private void AppendOverride(StringBuilder page, string commandPath)
    {
        var file = Path.Combine(
            AppContext.BaseDirectory, "overrides", $"{Slug(commandPath.Replace(' ', '-'))}.md");

        if (File.Exists(file))
        {
            page.AppendLine(File.ReadAllText(file).TrimEnd());
            page.AppendLine();
        }
    }

    private static IEnumerable<DocumentedCommand> Flatten(DocumentedCommand command)
    {
        yield return command;

        foreach (var child in command.Subcommands)
        {
            foreach (var descendant in Flatten(child))
            {
                yield return descendant;
            }
        }
    }

    private static string Synopsis(DocumentedCommand root, DocumentedCommand command, bool isRoot)
    {
        var name = isRoot ? root.Name : $"{root.Name} {command.Name}";
        var parts = command.Options
            .Select(option => option.IsRequired
                ? $"{option.CanonicalAlias} <{option.ValueType}>"
                : option.IsFlag
                    ? $"[{option.CanonicalAlias}]"
                    : $"[{option.CanonicalAlias} <{option.ValueType}>]");

        return $"{name} {string.Join(' ', parts)}".TrimEnd();
    }

    private static IEnumerable<(string Name, object? Value)> Constants(Type type) =>
        type.GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.IsLiteral)
            .Select(f => (f.Name, f.GetRawConstantValue()));

    private static string Slug(string value) => value.ToLowerInvariant().Replace(' ', '-');

    private static string Lowercase(string value) =>
        string.IsNullOrEmpty(value) ? value : char.ToLowerInvariant(value[0]) + value[1..];

    private static string Summarize(string description) =>
        string.IsNullOrWhiteSpace(description) ? "Command reference." : description;
}
