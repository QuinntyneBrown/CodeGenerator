// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.React.Syntax;
using CodeGenerator.Core.Syntax;

namespace CodeGenerator.React.UnitTests;

public class ErrorBoundaryModelTests
{
    [Fact]
    public void DefaultConstructor_SetsNameToEmptyString()
    {
        var model = new ErrorBoundaryModel();

        Assert.Equal(string.Empty, model.Name);
    }

    [Fact]
    public void DefaultConstructor_InitializesEmptyImports()
    {
        var model = new ErrorBoundaryModel();

        Assert.NotNull(model.Imports);
        Assert.Empty(model.Imports);
    }

    [Fact]
    public void DefaultConstructor_SetsFallbackContentToEmptyString()
    {
        var model = new ErrorBoundaryModel();

        Assert.Equal(string.Empty, model.FallbackContent);
    }

    [Fact]
    public void DefaultConstructor_IncludeRetryButton_DefaultsTrue()
    {
        var model = new ErrorBoundaryModel();

        Assert.True(model.IncludeRetryButton);
    }

    [Fact]
    public void DefaultConstructor_LogErrors_DefaultsTrue()
    {
        var model = new ErrorBoundaryModel();

        Assert.True(model.LogErrors);
    }

    [Fact]
    public void NamedConstructor_SetsName()
    {
        var model = new ErrorBoundaryModel("AppErrorBoundary");

        Assert.Equal("AppErrorBoundary", model.Name);
    }

    [Fact]
    public void NamedConstructor_InitializesEmptyImports()
    {
        var model = new ErrorBoundaryModel("AppErrorBoundary");

        Assert.NotNull(model.Imports);
        Assert.Empty(model.Imports);
    }

    [Fact]
    public void NamedConstructor_SetsFallbackContentToEmptyString()
    {
        var model = new ErrorBoundaryModel("AppErrorBoundary");

        Assert.Equal(string.Empty, model.FallbackContent);
    }

    [Fact]
    public void NamedConstructor_IncludeRetryButton_DefaultsTrue()
    {
        var model = new ErrorBoundaryModel("AppErrorBoundary");

        Assert.True(model.IncludeRetryButton);
    }

    [Fact]
    public void NamedConstructor_LogErrors_DefaultsTrue()
    {
        var model = new ErrorBoundaryModel("AppErrorBoundary");

        Assert.True(model.LogErrors);
    }

    [Fact]
    public void IncludeRetryButton_CanBeDisabled()
    {
        var model = new ErrorBoundaryModel("AppErrorBoundary");
        model.IncludeRetryButton = false;

        Assert.False(model.IncludeRetryButton);
    }

    [Fact]
    public void LogErrors_CanBeDisabled()
    {
        var model = new ErrorBoundaryModel("AppErrorBoundary");
        model.LogErrors = false;

        Assert.False(model.LogErrors);
    }

    [Fact]
    public void FallbackContent_CanBeSet()
    {
        var model = new ErrorBoundaryModel("AppErrorBoundary");
        model.FallbackContent = "<h1>Something went wrong</h1>";

        Assert.Equal("<h1>Something went wrong</h1>", model.FallbackContent);
    }

    [Fact]
    public void Imports_CanAddImport()
    {
        var model = new ErrorBoundaryModel("AppErrorBoundary");
        model.Imports.Add(new ImportModel("Component", "react"));

        Assert.Single(model.Imports);
    }

    [Fact]
    public void InheritsFromSyntaxModel()
    {
        var model = new ErrorBoundaryModel();

        Assert.IsAssignableFrom<SyntaxModel>(model);
    }

    [Fact]
    public void Name_CanBeUpdated()
    {
        var model = new ErrorBoundaryModel("Original");
        model.Name = "Updated";

        Assert.Equal("Updated", model.Name);
    }
}
