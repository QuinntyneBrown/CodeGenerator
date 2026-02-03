// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.DotNet.Syntax.Attributes;

/// <summary>
/// Initializes a new instance of the <see cref="ProducesResponseTypeAttributeModel"/> class.
/// </summary>
/// <param name="httpStatusCodeName">The http status code name.</param>
/// <param name="typeName">The type name.</param>
public class ProducesResponseTypeAttributeModel(string httpStatusCodeName, string typeName = null) : AttributeModel
{
    public string HttpStatusCodeName { get; set; } = httpStatusCodeName;

    public string TypeName { get; set; } = typeName;
}
