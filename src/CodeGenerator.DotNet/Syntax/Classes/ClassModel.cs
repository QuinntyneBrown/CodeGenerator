// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using CodeGenerator.DotNet.Syntax.Attributes;
using CodeGenerator.DotNet.Syntax.Constructors;
using CodeGenerator.DotNet.Syntax.Fields;
using CodeGenerator.DotNet.Syntax.Interfaces;
using CodeGenerator.DotNet.Syntax.Methods;

namespace CodeGenerator.DotNet.Syntax.Classes;

public class ClassModel : InterfaceModel
{
    public ClassModel()
    {
        Fields = [];
        Constructors = [];
        Attributes = [];
        AccessModifier = AccessModifier.Public;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ClassModel"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    public ClassModel(string name)
        : base(name)
    {
        Fields = [];
        Constructors = [];
        Attributes = [];
        AccessModifier = AccessModifier.Public;
    }

    public AccessModifier AccessModifier { get; set; }

    public List<FieldModel> Fields { get; set; }

    public List<ConstructorModel> Constructors { get; set; }

    public List<AttributeModel> Attributes { get; set; }

    public bool Static { get; set; }

    public bool Sealed { get; set; }

    public string BaseClass { get; set; }

    public override void AddMethod(MethodModel method)
    {
        method.Interface = false;
        Methods.Add(method);
    }

    public ClassModel CreateDto()
        => new ClassModel($"{Name}Dto")
        {
            Properties = Properties,
        };

    public override IEnumerable<SyntaxModel> GetChildren()
    {
        foreach (var method in Methods)
        {
            yield return method;
        }

        foreach (var method in Fields)
        {
            yield return method;
        }

        foreach (var method in Constructors)
        {
            yield return method;
        }

        foreach (var method in Attributes)
        {
            yield return method;
        }

        foreach (var implements in Implements)
        {
            yield return implements;
        }
    }
}
