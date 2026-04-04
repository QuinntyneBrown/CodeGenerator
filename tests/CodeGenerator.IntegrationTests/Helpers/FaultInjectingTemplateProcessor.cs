// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Errors;
using CodeGenerator.Core.Services;

namespace CodeGenerator.IntegrationTests.Helpers;

public class FaultInjectingTemplateProcessor : ITemplateProcessor
{
    private readonly ITemplateProcessor _inner;
    private readonly FaultInjectionOptions _options;
    private readonly Random _random;

    public FaultInjectingTemplateProcessor(ITemplateProcessor inner, FaultInjectionOptions options)
    {
        _inner = inner;
        _options = options;
        _random = options.RandomSeed.HasValue
            ? new Random(options.RandomSeed.Value)
            : new Random();
    }

    public string Process(string template, IDictionary<string, object> tokens, string[] ignoreTokens = null)
    {
        MaybeThrow(template);
        return _inner.Process(template, tokens, ignoreTokens);
    }

    public string Process(string template, IDictionary<string, object> tokens)
    {
        MaybeThrow(template);
        return _inner.Process(template, tokens);
    }

    public string Process(string template, dynamic model)
    {
        MaybeThrow(template);
        return _inner.Process(template, (object)model);
    }

    public Task<string> ProcessAsync(string template, IDictionary<string, object> tokens, string[] ignoreTokens = null)
    {
        MaybeThrow(template);
        return _inner.ProcessAsync(template, tokens, ignoreTokens);
    }

    public Task<string> ProcessAsync(string template, IDictionary<string, object> tokens)
    {
        MaybeThrow(template);
        return _inner.ProcessAsync(template, tokens);
    }

    public Task<string> ProcessAsync(string template, dynamic model)
    {
        MaybeThrow(template);
        return _inner.ProcessAsync(template, (object)model);
    }

    private void MaybeThrow(string template)
    {
        if (_options.SimulatedLatency.HasValue)
        {
            Thread.Sleep(_options.SimulatedLatency.Value);
        }

        if (_options.TemplateRenderFailureRate > 0.0 && _random.NextDouble() < _options.TemplateRenderFailureRate)
        {
            throw new CliTemplateException(
                $"Simulated template render failure for template ({template.Length} chars)");
        }
    }
}
