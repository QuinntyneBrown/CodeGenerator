// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Services;

public class NoOpCommandService : ICommandService
{
    public int Start(string command, string workingDirectory = null, bool waitForExit = true)
    {
        return 0;
    }
}
