// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.IntegrationTests.Helpers;

public class FaultInjectionOptions
{
    public double FileWriteFailureRate { get; set; } = 0.0;

    public double TemplateRenderFailureRate { get; set; } = 0.0;

    public double ProcessExecutionFailureRate { get; set; } = 0.0;

    public TimeSpan? SimulatedLatency { get; set; }

    public bool SimulateDiskFull { get; set; }

    public bool SimulatePermissionDenied { get; set; }

    public int? RandomSeed { get; set; }

    public List<string> TargetPaths { get; set; } = [];
}
