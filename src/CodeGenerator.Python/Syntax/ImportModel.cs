// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Python.Syntax;

public class ImportModel
{
    public ImportModel()
    {
        Names = [];
        Module = string.Empty;
    }

    public ImportModel(string module)
    {
        Module = module;
        Names = [];
    }

    public ImportModel(string module, params string[] names)
    {
        Module = module;
        Names = [.. names];
    }

    /// <summary>
    /// Gets or sets the module to import from (e.g., "flask", "os.path").
    /// </summary>
    public string Module { get; set; }

    /// <summary>
    /// Gets or sets the specific names to import from the module.
    /// When empty, generates "import Module". When populated, generates "from Module import Name1, Name2".
    /// </summary>
    public List<string> Names { get; set; }

    /// <summary>
    /// Gets or sets an optional alias for the import (e.g., "import numpy as np").
    /// </summary>
    public string? Alias { get; set; }
}
