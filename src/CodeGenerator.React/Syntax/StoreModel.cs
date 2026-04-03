// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.React.Syntax;

public class StoreModel : SyntaxModel
{
    public StoreModel(string name)
    {
        Name = name;
        StateProperties = [];
        Actions = [];
        ActionImplementations = new();
        ActionSignatures = new();
    }

    public string Name { get; set; }

    public List<PropertyModel> StateProperties { get; set; }

    public List<string> Actions { get; set; }

    public Dictionary<string, string> ActionImplementations { get; set; }

    /// <summary>
    /// Maps action name to its full TypeScript signature string.
    /// E.g., "fetchUsers" -> "(page?: number, perPage?: number) => Promise&lt;void&gt;"
    /// When present, uses this instead of generic "(...args: any[]) => void".
    /// </summary>
    public Dictionary<string, string> ActionSignatures { get; set; }

    public string? EntityName { get; set; }

    public bool IncludeAsyncState { get; set; } = false;
}
