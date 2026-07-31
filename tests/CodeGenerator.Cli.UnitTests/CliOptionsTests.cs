// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.CommandLine;
using System.Text.RegularExpressions;
using CodeGenerator.Cli.Commands;
using CodeGenerator.Cli.Formatting;
using CodeGenerator.Core.Errors;
using CodeGenerator.Core.Validation;

namespace CodeGenerator.Cli.UnitTests;

public class CliOptionsTests
{
    [Fact]
    public void ToolCommandName_MatchesProjectFile()
    {
        // The documentation site and every example invocation use this verb. If the
        // csproj is renamed without updating the constant, generated docs teach a
        // command that does not exist.
        var csproj = File.ReadAllText(TestPaths.Combine("src", "CodeGenerator.Cli", "CodeGenerator.Cli.csproj"));
        var declared = Regex.Match(csproj, @"<ToolCommandName>(?<name>[^<]+)</ToolCommandName>");

        Assert.True(declared.Success, "CodeGenerator.Cli.csproj declares no <ToolCommandName>.");
        Assert.Equal(declared.Groups["name"].Value, CliOptions.ToolCommandName);
    }

    [Fact]
    public void CreateVerbose_DeclaresBothAliases()
    {
        var option = CliOptions.CreateVerbose();

        Assert.Contains("--verbose", option.Aliases);
        Assert.Contains("-v", option.Aliases);
    }

    [Fact]
    public void CreateVerbose_HasDescription()
    {
        Assert.False(string.IsNullOrWhiteSpace(CliOptions.CreateVerbose().Description));
    }

    [Fact]
    public void EveryOptionInTheCommandTree_HasADescription()
    {
        // An option with no description cannot be documented, so the reference page
        // would ship an empty table cell.
        var root = CliTestTree.Build();

        foreach (var (path, option) in CliTestTree.Walk(root))
        {
            Assert.False(
                string.IsNullOrWhiteSpace(option.Description),
                $"Option '{option.Aliases.First()}' on '{path}' has no description.");
        }
    }
}

public class ExitCodeMapperTests
{
    private static readonly IErrorFormatter Formatter = new ConsoleErrorFormatter();

    [Fact]
    public void Map_CliException_ReturnsItsExitCode()
    {
        var writer = new StringWriter();

        var code = ExitCodeMapper.Map(new CliTemplateException("boom"), verbose: false, Formatter, writer);

        Assert.Equal(CliExitCodes.TemplateError, code);
        Assert.Contains("boom", writer.ToString());
    }

    [Fact]
    public void Map_ValidationExceptionWithResult_WritesEachError()
    {
        var result = new ValidationResult();
        result.AddError("Name", "Solution name is required.");
        var writer = new StringWriter();

        var code = ExitCodeMapper.Map(new CliValidationException(result), verbose: false, Formatter, writer);

        Assert.Equal(CliExitCodes.ValidationError, code);
        Assert.Contains("Name", writer.ToString());
        Assert.Contains("Solution name is required.", writer.ToString());
    }

    [Fact]
    public void Map_AggregateException_ReturnsHighestInnerExitCode()
    {
        var aggregate = new CliAggregateException(
        [
            new CliValidationException("first"),
            new CliPluginException("second"),
        ]);
        var writer = new StringWriter();

        var code = ExitCodeMapper.Map(aggregate, verbose: false, Formatter, writer);

        Assert.Equal(CliExitCodes.PluginError, code);
    }

    [Fact]
    public void Map_OperationCanceled_ReturnsCancelled()
    {
        var writer = new StringWriter();

        var code = ExitCodeMapper.Map(new OperationCanceledException(), verbose: false, Formatter, writer);

        Assert.Equal(CliExitCodes.Cancelled, code);
        Assert.Contains("cancelled", writer.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Map_UnknownException_ReturnsUnexpectedAndHidesStackTraceUnlessVerbose()
    {
        var quiet = new StringWriter();
        var loud = new StringWriter();
        var exception = new InvalidOperationException("internal detail");

        Assert.Equal(CliExitCodes.UnexpectedError, ExitCodeMapper.Map(exception, false, Formatter, quiet));
        Assert.Equal(CliExitCodes.UnexpectedError, ExitCodeMapper.Map(exception, true, Formatter, loud));

        Assert.DoesNotContain("internal detail", quiet.ToString());
        Assert.Contains("--verbose", quiet.ToString());
        Assert.Contains("internal detail", loud.ToString());
    }

    [Fact]
    public void Map_UnknownException_UsesTheInternalErrorCode()
    {
        var writer = new StringWriter();

        ExitCodeMapper.Map(new Exception("x"), verbose: false, Formatter, writer);

        Assert.Contains(ErrorCodes.InternalUnexpected, writer.ToString());
    }
}
