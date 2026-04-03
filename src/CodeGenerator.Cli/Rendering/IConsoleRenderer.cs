// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Cli.Rendering;

public interface IConsoleRenderer
{
    void WriteHeader(string title);

    void WriteStep(int current, int total, string description);

    void WriteStepComplete(int current, int total, string description);

    void WriteSuccess(string message);

    void WriteError(string message);

    void WriteWarning(string message);

    void WriteInfo(string message);

    void WriteTree(string rootLabel, IReadOnlyList<GeneratedFileEntry> files);

    void WriteSummary(GenerationResult result);

    void WriteLine();
}
