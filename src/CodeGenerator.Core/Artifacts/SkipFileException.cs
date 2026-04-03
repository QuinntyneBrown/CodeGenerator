// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Artifacts;

public class SkipFileException : Exception
{
    public SkipFileException()
        : base("Template signaled to skip file generation.") { }

    public SkipFileException(string reason)
        : base(reason) { }
}
