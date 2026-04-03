// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Schema;

public interface ISchemaNormalizer
{
    SchemaFormat Format { get; }
    bool CanNormalize(string content, string? filePath = null);
    Task<NormalizedSchema> NormalizeAsync(string content, SchemaFormat format,
        CancellationToken cancellationToken = default);
}
