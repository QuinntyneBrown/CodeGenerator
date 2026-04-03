// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Abstractions.Results;
using CodeGenerator.Core.Validation;
using Xunit.Sdk;

namespace CodeGenerator.IntegrationTests.Helpers;

public static class ResultAssertions
{
    public static void ShouldBeSuccess<T>(this Result<T> result)
    {
        if (!result.IsSuccess)
        {
            throw new XunitException(
                $"Expected result to be successful, but it failed with error: [{result.Error.Code}] {result.Error.Message}");
        }
    }

    public static T ShouldBeSuccessWithValue<T>(this Result<T> result)
    {
        result.ShouldBeSuccess();
        return result.Value;
    }

    public static void ShouldBeFailure<T>(this Result<T> result)
    {
        if (!result.IsFailure)
        {
            throw new XunitException(
                "Expected result to be a failure, but it was successful.");
        }
    }

    public static void ShouldBeFailureWithCode<T>(this Result<T> result, string code)
    {
        result.ShouldBeFailure();

        if (result.Error.Code != code)
        {
            throw new XunitException(
                $"Expected failure with code '{code}', but got '{result.Error.Code}'.");
        }
    }

    public static void ShouldBeSuccess(this Result result)
    {
        if (!result.IsSuccess)
        {
            throw new XunitException(
                $"Expected result to be successful, but it failed with error: [{result.Error.Code}] {result.Error.Message}");
        }
    }

    public static void ShouldBeFailure(this Result result)
    {
        if (!result.IsFailure)
        {
            throw new XunitException(
                "Expected result to be a failure, but it was successful.");
        }
    }

    public static void ShouldHaveValidationError(this ValidationResult result, string propertyName)
    {
        var hasError = result.Errors.Any(e => e.PropertyName == propertyName);

        if (!hasError)
        {
            var existing = result.Errors.Count > 0
                ? string.Join(", ", result.Errors.Select(e => e.PropertyName))
                : "(none)";

            throw new XunitException(
                $"Expected validation error for property '{propertyName}', but found errors for: {existing}.");
        }
    }

    public static void ShouldHaveNoErrors(this ValidationResult result)
    {
        if (!result.IsValid)
        {
            var errors = string.Join("; ", result.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));

            throw new XunitException(
                $"Expected no validation errors, but found: {errors}.");
        }
    }
}
