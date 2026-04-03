// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Errors;

public static class CliExitCodes
{
    public const int Success = 0;
    public const int ValidationError = 1;
    public const int IoError = 2;
    public const int ProcessError = 3;
    public const int TemplateError = 4;
    public const int UnexpectedError = 99;
}
