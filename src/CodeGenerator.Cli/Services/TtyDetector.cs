// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Cli.Services;

public static class TtyDetector
{
    public static bool IsInteractiveTerminal() => !Console.IsInputRedirected;
}
