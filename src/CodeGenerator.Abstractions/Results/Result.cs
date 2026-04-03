// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Abstractions.Results;

public class Result
{
    private readonly ErrorInfo? _error;

    private Result() { }

    private Result(ErrorInfo error)
    {
        _error = error;
    }

    public bool IsSuccess => _error is null;

    public bool IsFailure => !IsSuccess;

    public ErrorInfo Error => _error ?? throw new InvalidOperationException("Cannot access Error on a successful result.");

    public static Result Ok() => new();

    public static Result Fail(ErrorInfo error) => new(error);

    public Result<U> Bind<U>(Func<Result<U>> next) =>
        IsSuccess ? next() : Result<U>.Failure(_error!);
}
