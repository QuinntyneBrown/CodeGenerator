// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace CodeGenerator.DotNet.Syntax.Properties;

public class PropertyAccessorModel
{
    public PropertyAccessorModel(string accessModifier, PropertyAccessorType classPropertyAccessorType)
        : this(classPropertyAccessorType)
    {
        AccessModifier = accessModifier;
    }

    public PropertyAccessorModel(PropertyAccessorType classPropertyAccessorType)
    {
        Type = classPropertyAccessorType;
    }

    private PropertyAccessorModel()
    {
    }

    public static PropertyAccessorModel Get => new PropertyAccessorModel(PropertyAccessorType.Get);

    public static PropertyAccessorModel Set => new PropertyAccessorModel(PropertyAccessorType.Set);

    public static PropertyAccessorModel Init => new PropertyAccessorModel(PropertyAccessorType.Init);

    public static PropertyAccessorModel PrivateSet => new PropertyAccessorModel("private", PropertyAccessorType.Set);

    public static List<PropertyAccessorModel> GetPrivateSet => new List<PropertyAccessorModel>() { Get, PrivateSet };

    public static List<PropertyAccessorModel> GetSet => new List<PropertyAccessorModel>() { Get, Set };

    public static List<PropertyAccessorModel> GetInit => new List<PropertyAccessorModel>() { Get, Init };

    public string AccessModifier { get; private set; }

    public PropertyAccessorType Type { get; private set; }

    public static bool IsGetPrivateSet(List<PropertyAccessorModel> accessors)
    {
        return true;
    }
}
