// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using CodeGenerator.Core.Validation;
using CodeGenerator.DotNet.Syntax.Attributes;
using CodeGenerator.DotNet.Syntax.Constructors;
using CodeGenerator.DotNet.Syntax.Fields;
using CodeGenerator.DotNet.Syntax.Interfaces;
using CodeGenerator.DotNet.Syntax.Methods;
using CodeGenerator.DotNet.Syntax.Params;

namespace CodeGenerator.DotNet.Syntax.Classes;

public class ClassModel : InterfaceModel
{
    public ClassModel()
    {
        Fields = [];
        Constructors = [];
        Attributes = [];
        PrimaryConstructorParams = [];
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
        PrimaryConstructorParams = [];
        AccessModifier = AccessModifier.Public;
    }

    public AccessModifier AccessModifier { get; set; }

    public List<FieldModel> Fields { get; set; }

    public List<ConstructorModel> Constructors { get; set; }

    public List<AttributeModel> Attributes { get; set; }

    public List<ParamModel> PrimaryConstructorParams { get; set; }

    public List<string> GenericTypeParameters { get; set; } = [];

    public List<string> GenericConstraints { get; set; } = [];

    public bool Static { get; set; }

    public bool Abstract { get; set; }

    public bool Sealed { get; set; }

    public string BaseClass { get; set; }

    public List<ClassModel> InnerClasses { get; set; } = [];

    public override ValidationResult Validate()
    {
        var result = base.Validate();
        if (string.IsNullOrWhiteSpace(Name))
            result.AddError(nameof(Name), "Class name is required.");
        return result;
    }

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
        foreach (var field in Fields)
        {
            yield return field;
        }

        foreach (var constructor in Constructors)
        {
            yield return constructor;
        }

        foreach (var property in Properties)
        {
            yield return property;
        }

        foreach (var method in Methods)
        {
            yield return method;
        }

        foreach (var attribute in Attributes)
        {
            yield return attribute;
        }

        foreach (var implements in Implements)
        {
            yield return implements;
        }

        foreach (var innerClass in InnerClasses)
        {
            yield return innerClass;
        }
    }
}
