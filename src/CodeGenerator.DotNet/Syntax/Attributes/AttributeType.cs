// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.DotNet.Syntax.Attributes;

public enum AttributeType
{
    /// <summary>
    /// ApiVersion
    /// </summary>
    ApiVersion,

    /// <summary>
    /// Authorize
    /// </summary>
    Authorize,

    /// <summary>
    /// ApiController
    /// </summary>
    ApiController,

    /// <summary>
    /// Produces
    /// </summary>
    Produces,

    /// <summary>
    /// Consumes
    /// </summary>
    Consumes,

    /// <summary>
    /// ProducesResponseType
    /// </summary>
    ProducesResponseType,

    /// <summary>
    /// Http
    /// </summary>
    Http,

    /// <summary>
    /// SwaggerOperation
    /// </summary>
    SwaggerOperation,

    /// <summary>
    /// Route
    /// </summary>
    Route,

    /// <summary>
    /// Fact
    /// </summary>
    Fact,

    /// <summary>
    /// Test
    /// </summary>
    Test,
}
