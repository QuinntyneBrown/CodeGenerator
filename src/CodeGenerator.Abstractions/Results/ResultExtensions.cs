// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Abstractions.Results;

public static class ResultExtensions
{
    public static Result<T> OnSuccess<T>(this Result<T> result, Action<T> action)
    {
        if (result.IsSuccess)
            action(result.Value);
        return result;
    }

    public static Result<T> OnFailure<T>(this Result<T> result, Action<ErrorInfo> action)
    {
        if (result.IsFailure)
            action(result.Error);
        return result;
    }

    public static Result Combine(this IEnumerable<Result> results)
    {
        var failures = results.Where(r => r.IsFailure).ToList();
        if (failures.Count == 0)
            return Result.Ok();

        return Result.Fail(failures[0].Error);
    }

    public static Result<T> ToResult<T>(this T? value, ErrorInfo errorIfNull) where T : class =>
        value is not null ? Result<T>.Success(value) : Result<T>.Failure(errorIfNull);
}
