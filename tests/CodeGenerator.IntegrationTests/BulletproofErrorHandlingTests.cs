// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core;
using CodeGenerator.Core.Errors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CodeGenerator.IntegrationTests;

public class BulletproofErrorHandlingTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;

    public BulletproofErrorHandlingTests()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => { builder.AddConsole(); builder.SetMinimumLevel(LogLevel.Warning); });
        services.AddCoreServices(typeof(BulletproofErrorHandlingTests).Assembly);
        services.AddDotNetServices();
        _serviceProvider = services.BuildServiceProvider();
    }

    public void Dispose() => _serviceProvider.Dispose();

    [Fact]
    public void CliExitCodes_Constants()
    {
        Assert.Equal(0, CliExitCodes.Success);
        Assert.Equal(1, CliExitCodes.ValidationError);
        Assert.Equal(2, CliExitCodes.IoError);
        Assert.Equal(3, CliExitCodes.ProcessError);
        Assert.Equal(4, CliExitCodes.TemplateError);
        Assert.Equal(99, CliExitCodes.UnexpectedError);
    }

    [Fact]
    public void CliValidationException_HasCorrectExitCode()
    {
        var ex = new CliValidationException("bad input");
        Assert.Equal(CliExitCodes.ValidationError, ex.ExitCode);
    }

    [Fact]
    public void CliIOException_HasCorrectExitCode()
    {
        var ex = new CliIOException("file not found");
        Assert.Equal(CliExitCodes.IoError, ex.ExitCode);
    }

    [Fact]
    public void CliTemplateException_HasCorrectExitCode()
    {
        var ex = new CliTemplateException("render error");
        Assert.Equal(CliExitCodes.TemplateError, ex.ExitCode);
    }

    [Fact]
    public void GenerationRollbackService_TracksAndRollsBack()
    {
        var rollback = _serviceProvider.GetRequiredService<IGenerationRollbackService>();
        var tempFile = Path.GetTempFileName();

        try
        {
            rollback.TrackFile(tempFile);
            Assert.True(File.Exists(tempFile));

            rollback.Rollback();
            Assert.False(File.Exists(tempFile));
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void GenerationRollbackService_CommitPreventsRollback()
    {
        var rollback = _serviceProvider.GetRequiredService<IGenerationRollbackService>();
        var tempFile = Path.GetTempFileName();

        try
        {
            rollback.TrackFile(tempFile);
            rollback.Commit();
            rollback.Rollback();

            Assert.True(File.Exists(tempFile));
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }
}
