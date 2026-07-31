// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.CommandLine;

namespace CodeGenerator.Cli.Commands;

/// <summary>
/// Command-line surface shared between the running tool and any consumer that needs to
/// inspect the command tree, such as the documentation generator in <c>eng/DocsGen</c>.
/// </summary>
public static class CliOptions
{
    /// <summary>
    /// The verb the packed global tool is invoked by. Kept in step with
    /// <c>&lt;ToolCommandName&gt;</c> in <c>CodeGenerator.Cli.csproj</c> by
    /// <c>CliOptionsTests</c>.
    /// </summary>
    public const string ToolCommandName = "create-code-cli";

    /// <summary>
    /// Creates the global verbose option. Defined here rather than inline in
    /// <c>Program</c> so that a consumer building the command tree for inspection sees
    /// the same option the running tool registers.
    /// </summary>
    public static Option<bool> CreateVerbose() => new(
        aliases: ["--verbose", "-v"],
        description: "Show detailed error output and stack traces");
}
