// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Diagnostics;

public class DiagnosticContext
{
    private static readonly AsyncLocal<DiagnosticContext> _current = new();

    public static DiagnosticContext Current
    {
        get
        {
            _current.Value ??= new DiagnosticContext();
            return _current.Value;
        }
    }

    public string? CorrelationId { get; set; }

    public string? CurrentFile { get; set; }

    public string? CurrentStrategy { get; set; }

    public DiagnosticPhase? CurrentPhase { get; set; }

    public string? ModelType { get; set; }

    public Dictionary<string, object> Properties { get; } = [];

    public void Reset()
    {
        CorrelationId = null;
        CurrentFile = null;
        CurrentStrategy = null;
        CurrentPhase = null;
        ModelType = null;
        Properties.Clear();
    }
}
