// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Errors;

namespace CodeGenerator.Core.UnitTests;

public class CliExitCodesTests
{
    [Fact]
    public void Success_IsZero()
    {
        Assert.Equal(0, CliExitCodes.Success);
    }

    [Fact]
    public void ValidationError_IsOne()
    {
        Assert.Equal(1, CliExitCodes.ValidationError);
    }

    [Fact]
    public void IoError_IsTwo()
    {
        Assert.Equal(2, CliExitCodes.IoError);
    }

    [Fact]
    public void ProcessError_IsThree()
    {
        Assert.Equal(3, CliExitCodes.ProcessError);
    }

    [Fact]
    public void TemplateError_IsFour()
    {
        Assert.Equal(4, CliExitCodes.TemplateError);
    }

    [Fact]
    public void UnexpectedError_Is99()
    {
        Assert.Equal(99, CliExitCodes.UnexpectedError);
    }

    [Fact]
    public void AllCodes_AreDistinct()
    {
        var codes = new[]
        {
            CliExitCodes.Success,
            CliExitCodes.ValidationError,
            CliExitCodes.IoError,
            CliExitCodes.ProcessError,
            CliExitCodes.TemplateError,
            CliExitCodes.UnexpectedError
        };
        Assert.Equal(codes.Length, codes.Distinct().Count());
    }
}
