// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;

namespace CodeGenerator.Core.Schema;

public class SchemaNormalizerDispatcher
{
    private readonly IEnumerable<ISchemaNormalizer> _normalizers;
    private readonly ISchemaFormatDetector _formatDetector;
    private readonly ILogger<SchemaNormalizerDispatcher> _logger;

    public SchemaNormalizerDispatcher(
        IEnumerable<ISchemaNormalizer> normalizers,
        ISchemaFormatDetector formatDetector,
        ILogger<SchemaNormalizerDispatcher> logger)
    {
        _normalizers = normalizers;
        _formatDetector = formatDetector;
        _logger = logger;
    }

    public async Task<NormalizedSchema> NormalizeAsync(
        string content,
        SchemaFormat? formatHint = null,
        string? filePath = null,
        CancellationToken cancellationToken = default)
    {
        var format = formatHint ?? _formatDetector.Detect(content, filePath);

        var normalizer = _normalizers.FirstOrDefault(n => n.Format == format)
            ?? throw new UnsupportedSchemaFormatException(format);

        _logger.LogInformation("Normalizing {Format} schema using {Normalizer}.",
            format, normalizer.GetType().Name);

        return await normalizer.NormalizeAsync(content, format, cancellationToken);
    }
}
