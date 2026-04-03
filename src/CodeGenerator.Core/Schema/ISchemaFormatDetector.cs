// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Schema;

public interface ISchemaFormatDetector
{
    SchemaFormat Detect(string content, string? filePath = null);
}

public class SchemaFormatDetectionException : Exception
{
    public SchemaFormatDetectionException(string message) : base(message) { }
}

public class UnsupportedSchemaFormatException : Exception
{
    public UnsupportedSchemaFormatException(SchemaFormat format)
        : base($"No normalizer registered for schema format '{format}'.") { }
}
