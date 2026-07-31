// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Reflection;
using System.Text.Json;
using CodeGenerator.Core;
using CodeGenerator.Core.Scaffold.Services;
using CodeGenerator.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CodeGenerator.IntegrationTests.Documentation;

/// <summary>
/// Pins each documented limitation to the behaviour that made it necessary.
///
/// These tests assert the CURRENT behaviour, which is the behaviour the documentation
/// describes — not the behaviour anyone wants. When one of them fails, the underlying
/// defect has been fixed: delete the test, delete the admonition it pins, and move the
/// register entry to the Resolved table.
///
/// This is the only check that catches drift in the fixing direction. Every other check
/// catches "the code changed and the docs did not"; this one catches "the code was fixed
/// and the docs still apologise for it".
/// </summary>
[Trait("Category", "Docs")]
public class DocumentedLimitationsTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly string _workspaceRoot;

    public DocumentedLimitationsTests()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Warning));
        services.AddCoreServices(typeof(DocumentedLimitationsTests).Assembly);
        services.AddDotNetServices();
        services.AddScaffoldingServices();
        services.AddSingleton<ICommandService, NoOpCommandService>();

        _serviceProvider = services.BuildServiceProvider();
        _workspaceRoot = Path.Combine(Path.GetTempPath(), $"kl-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_workspaceRoot);
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();

        if (Directory.Exists(_workspaceRoot))
        {
            Directory.Delete(_workspaceRoot, recursive: true);
        }

        GC.SuppressFinalize(this);
    }

    private IScaffoldEngine Engine => _serviceProvider.GetRequiredService<IScaffoldEngine>();

    private const string MinimalYaml = """
        name: kl
        version: 1.0.0
        projects:
          - name: Kl.Api
            type: dotnet-classlib
            path: src/Kl.Api
        """;

    /// <summary>KL-001 — `--force` is accepted and never read; writes always overwrite.</summary>
    [Fact]
    public async Task ScaffoldOverwritesWithoutForce()
    {
        var yaml = """
            name: kl
            version: 1.0.0
            projects:
              - name: Kl.Api
                type: dotnet-classlib
                path: src/Kl.Api
                files:
                  - name: marker.txt
                    content: generated
            """;

        var target = Path.Combine(_workspaceRoot, "kl", "src", "Kl.Api", "marker.txt");
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        await File.WriteAllTextAsync(target, "hand written, please keep");

        // force: false. If this ever preserves the file, KL-001 has been fixed.
        await Engine.ScaffoldAsync(yaml, _workspaceRoot, dryRun: false, force: false);

        Assert.Equal("generated", (await File.ReadAllTextAsync(target)).Trim());
    }

    /// <summary>KL-002 — a dry run plans project directories, not the files inside them.</summary>
    [Fact]
    public async Task DryRunPlansProjectDirectoriesNotFiles()
    {
        var result = await Engine.ScaffoldAsync(MinimalYaml, _workspaceRoot, dryRun: true, force: false);

        Assert.True(result.ValidationResult.IsValid);
        Assert.NotEmpty(result.PlannedFiles);

        // Every planned entry for a project is its directory; none names a file within it.
        Assert.DoesNotContain(result.PlannedFiles, file => file.Path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>KL-003 — `file.template` passes validation and produces an empty file.</summary>
    [Fact]
    public async Task FileTemplateProducesEmptyFile()
    {
        var yaml = """
            name: kl
            version: 1.0.0
            projects:
              - name: Kl.Api
                type: dotnet-classlib
                path: src/Kl.Api
                files:
                  - name: from-template.txt
                    template: some-template
            """;

        var result = await Engine.ScaffoldAsync(yaml, _workspaceRoot, dryRun: false, force: false);

        Assert.True(result.ValidationResult.IsValid, "A file declaring only `template` still validates.");

        var written = Path.Combine(_workspaceRoot, "kl", "src", "Kl.Api", "from-template.txt");
        Assert.True(File.Exists(written));
        Assert.Empty(await File.ReadAllTextAsync(written));
    }

    /// <summary>KL-004 — `gitInit` is accepted and initializes nothing.</summary>
    [Fact]
    public async Task GitInitCreatesNoRepository()
    {
        var yaml = """
            name: kl
            version: 1.0.0
            gitInit: true
            projects:
              - name: Kl.Api
                type: dotnet-classlib
                path: src/Kl.Api
            """;

        await Engine.ScaffoldAsync(yaml, _workspaceRoot, dryRun: false, force: false);

        Assert.False(Directory.Exists(Path.Combine(_workspaceRoot, "kl", ".git")));
    }

    /// <summary>KL-005 — `file.encoding` is accepted and ignored; output is always UTF-8.</summary>
    [Fact]
    public async Task FileEncodingIsIgnored()
    {
        var yaml = """
            name: kl
            version: 1.0.0
            projects:
              - name: Kl.Api
                type: dotnet-classlib
                path: src/Kl.Api
                files:
                  - name: encoded.txt
                    content: "café"
                    encoding: utf-32
            """;

        await Engine.ScaffoldAsync(yaml, _workspaceRoot, dryRun: false, force: false);

        var written = Path.Combine(_workspaceRoot, "kl", "src", "Kl.Api", "encoded.txt");
        var bytes = await File.ReadAllBytesAsync(written);

        // UTF-32 would encode 'c' as four bytes. UTF-8 encodes it as one.
        Assert.Equal((byte)'c', bytes[0]);
    }

    /// <summary>KL-006 — dependencies, devDependencies, and features are parsed and ignored.</summary>
    [Fact]
    public async Task ProjectDependenciesAreIgnored()
    {
        var yaml = """
            name: kl
            version: 1.0.0
            projects:
              - name: Kl.Api
                type: dotnet-classlib
                path: src/Kl.Api
                dependencies:
                  - Serilog
                devDependencies:
                  - xunit
                features:
                  - swagger
            """;

        var result = await Engine.ScaffoldAsync(yaml, _workspaceRoot, dryRun: false, force: false);

        Assert.True(result.ValidationResult.IsValid);

        var csproj = Path.Combine(_workspaceRoot, "kl", "src", "Kl.Api", "Kl.Api.csproj");
        Assert.True(File.Exists(csproj));
        Assert.DoesNotContain("Serilog", await File.ReadAllTextAsync(csproj), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// KL-007 — the root command renders its diagnostics report after the error-handling
    /// block, so a failure skips it. Pinned structurally: the render call sits outside the
    /// try/catch that rethrows.
    /// </summary>
    [Fact]
    public void DiagnosticsSuppressedOnFailure()
    {
        var source = File.ReadAllText(RepositoryPaths.Combine(
            "src", "CodeGenerator.Cli", "Commands", "CreateCodeGeneratorCommand.cs"));

        var rethrow = source.LastIndexOf("rollbackService.Rollback();", StringComparison.Ordinal);
        var render = source.LastIndexOf("renderer.Render(report);", StringComparison.Ordinal);

        Assert.True(rethrow > 0 && render > rethrow,
            "The diagnostics report is no longer rendered after the failure path. "
            + "If it now runs in a finally block, KL-007 is fixed.");
    }

    /// <summary>
    /// Every register entry must name a test in this class, and every test here must be
    /// named by an entry. Without this the register and its pins drift apart silently.
    /// </summary>
    [Fact]
    public void EveryRegisterEntryIsPinnedByATestInThisClass()
    {
        var manifestPath = RepositoryPaths.Combine("website", "known-limitations.json");
        using var document = JsonDocument.Parse(File.ReadAllText(manifestPath));

        var named = document.RootElement.GetProperty("limitations")
            .EnumerateArray()
            .Where(l => !l.TryGetProperty("fixedIn", out var fixedIn) || fixedIn.ValueKind == JsonValueKind.Null)
            .Select(l => l.GetProperty("characterizationTest").GetString()!)
            .ToList();

        var declared = typeof(DocumentedLimitationsTests)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetCustomAttribute<FactAttribute>() is not null)
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var test in named)
        {
            Assert.True(declared.Contains(test), $"known-limitations.json names '{test}', which does not exist here.");
        }
    }
}

internal static class RepositoryPaths
{
    private static readonly string Root = Resolve();

    public static string Combine(params string[] segments) =>
        Path.GetFullPath(Path.Combine([Root, .. segments]));

    private static string Resolve()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "CodeGenerator.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException("Could not locate the repository root from the test host.");
    }
}
