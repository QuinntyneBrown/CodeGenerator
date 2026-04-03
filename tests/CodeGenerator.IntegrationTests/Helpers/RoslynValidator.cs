// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace CodeGenerator.IntegrationTests.Helpers;

public static class RoslynValidator
{
    public static IReadOnlyList<Diagnostic> ValidateCSharpSyntax(string sourceText)
    {
        var tree = CSharpSyntaxTree.ParseText(sourceText);
        return tree.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();
    }

    public static void AssertValidCSharp(string sourceText, string context = "")
    {
        var errors = ValidateCSharpSyntax(sourceText);
        Assert.True(errors.Count == 0,
            $"C# syntax errors{(string.IsNullOrEmpty(context) ? "" : $" in {context}")}:\n" +
            string.Join("\n", errors.Select(e => $"  {e.GetMessage()}")));
    }
}
