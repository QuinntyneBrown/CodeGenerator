// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Validation;

namespace CodeGenerator.React.Syntax;

public class ComponentModel : SyntaxModel
{
    public ComponentModel(string name)
    {
        Name = name;
        Props = [];
        Hooks = [];
        Children = [];
        Imports = [];
    }

    public string Name { get; set; }

    public List<PropertyModel> Props { get; set; }

    public List<string> Hooks { get; set; }

    public List<string> Children { get; set; }

    public List<ImportModel> Imports { get; set; }

    public bool IsClient { get; set; }

    /// <summary>
    /// Component rendering style: "forwardRef" (default, current), "fc" (React.FC), or "arrow" (bare arrow function).
    /// </summary>
    public string ComponentStyle { get; set; } = "forwardRef";

    /// <summary>
    /// Custom JSX body content to render inside the component. Overrides default empty shell.
    /// </summary>
    public string? BodyContent { get; set; }

    /// <summary>
    /// Whether to use `export default ComponentName` at the end. Default false (uses named export).
    /// </summary>
    public bool ExportDefault { get; set; }

    public bool IncludeChildren { get; set; } = false;

    public string RefElementType { get; set; } = "HTMLDivElement";

    public bool UseMemo { get; set; }

    public bool SpreadProps { get; set; }

    public override ValidationResult Validate()
    {
        var result = new ValidationResult();
        if (string.IsNullOrWhiteSpace(Name))
            result.AddError(nameof(Name), "Component name is required.");
        return result;
    }
}
