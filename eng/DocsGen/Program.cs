// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.DocsGen;

var repositoryRoot = RepositoryLocator.Find();
var docsRoot = ArgumentOf(args, "--output")
    ?? Path.Combine(repositoryRoot, "website", "src", "content", "docs");
var check = args.Contains("--check");
MarkdownEmitter.CheckOnly = check;

var limitations = KnownLimitations.Load(Path.Combine(repositoryRoot, "website", "known-limitations.json"));

if (limitations.All.Count == 0)
{
    Console.Error.WriteLine("WARNING: no known-limitations.json found; admonitions will be omitted.");
}

var written = new List<string>();
written.AddRange(new CliReferenceEmitter(docsRoot, limitations).Emit());
written.AddRange(new SchemaReferenceEmitter(docsRoot, limitations).Emit());
written.AddRange(new KnownLimitationsPageEmitter(docsRoot, limitations).Emit());
written.AddRange(new SpecMirrorEmitter(docsRoot, repositoryRoot).Emit());

foreach (var file in written)
{
    Console.WriteLine($"  wrote {Path.GetRelativePath(repositoryRoot, file).Replace('\\', '/')}");
}

if (written.Count == 0)
{
    Console.WriteLine("Generated documentation is up to date.");
    return 0;
}

if (check)
{
    Console.Error.WriteLine(
        $"ERROR: {written.Count} generated file(s) are out of date. "
        + "Run `dotnet run --project eng/DocsGen` and commit the result.");
    return 1;
}

Console.WriteLine($"Regenerated {written.Count} file(s).");
return 0;

static string? ArgumentOf(string[] args, string name)
{
    var index = Array.IndexOf(args, name);
    return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
}
