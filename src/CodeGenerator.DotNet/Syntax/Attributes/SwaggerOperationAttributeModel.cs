// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.DotNet.Syntax.Attributes;

public class SwaggerOperationAttributeModel(string summary, string description) : AttributeModel
{
    public string Summary { get; set; } = summary;

    public string Description { get; set; } = description;
}
