// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.React.Syntax;

public class HookModel : SyntaxModel
{
    public HookModel(string name)
    {
        Name = name;
        Params = [];
        Imports = [];
        Body = string.Empty;
        ReturnType = string.Empty;
        Effects = [];
        TypeParameters = [];
    }

    public string Name { get; set; }

    public List<PropertyModel> Params { get; set; }

    public string ReturnType { get; set; }

    public string Body { get; set; }

    public List<ImportModel> Imports { get; set; }

    public List<EffectDefinition> Effects { get; set; }

    public List<string> TypeParameters { get; set; }

    public class EffectDefinition
    {
        public string Body { get; set; } = string.Empty;

        public List<string> Dependencies { get; set; } = [];
    }
}
