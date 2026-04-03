// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;

namespace CodeGenerator.Core.Verification;

public class VerificationRunner
{
    private readonly IEnumerable<IPostGenerationVerifier> _verifiers;
    private readonly ILogger<VerificationRunner> _logger;

    public VerificationRunner(
        IEnumerable<IPostGenerationVerifier> verifiers,
        ILogger<VerificationRunner> logger)
    {
        _verifiers = verifiers;
        _logger = logger;
    }

    public async Task<VerificationResult> RunAllAsync(VerificationOptions options)
    {
        var result = new VerificationResult();
        var buildPassed = true;

        foreach (var verifier in _verifiers)
        {
            if (!buildPassed && verifier.Name != "dotnet build")
            {
                result.Steps.Add(new VerificationStepResult
                {
                    VerifierName = verifier.Name,
                    Passed = false,
                    FailureReason = "Skipped: build failed",
                    Duration = TimeSpan.Zero,
                });
                continue;
            }

            _logger.LogInformation("Running verification: {Name}", verifier.Name);

            var stepResult = await verifier.VerifyAsync(options.SolutionDirectory, options);
            result.Steps.Add(stepResult);

            if (!stepResult.Passed && verifier.Name == "dotnet build")
            {
                buildPassed = false;
            }
        }

        return result;
    }
}
