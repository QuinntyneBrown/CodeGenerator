// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Cli.Rendering;

public class GenerationProgressReporter
{
    private readonly IConsoleRenderer _renderer;
    private readonly int _totalSteps;
    private int _currentStep;

    public GenerationProgressReporter(IConsoleRenderer renderer, int totalSteps)
    {
        _renderer = renderer;
        _totalSteps = totalSteps;
    }

    public void BeginStep(string description)
    {
        _currentStep++;
        _renderer.WriteStep(_currentStep, _totalSteps, description);
    }

    public void CompleteStep(string description)
    {
        _renderer.WriteStepComplete(_currentStep, _totalSteps, description);
    }

    public async Task<T> RunStepAsync<T>(string description, Func<Task<T>> action)
    {
        BeginStep(description);
        var result = await action();
        CompleteStep(description);
        return result;
    }

    public async Task RunStepAsync(string description, Func<Task> action)
    {
        BeginStep(description);
        await action();
        CompleteStep(description);
    }

    public void RunStep(string description, Action action)
    {
        BeginStep(description);
        action();
        CompleteStep(description);
    }
}
