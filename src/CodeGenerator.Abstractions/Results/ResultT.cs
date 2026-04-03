// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Abstractions.Results;

public class Result<T>
{
    private readonly T? _value;
    private readonly ErrorInfo? _error;
    private readonly bool _isSuccess;

    private Result(T value)
    {
        _value = value;
        _isSuccess = true;
    }

    private Result(ErrorInfo error)
    {
        _error = error;
        _isSuccess = false;
    }

    public bool IsSuccess => _isSuccess;

    public bool IsFailure => !_isSuccess;

    public T Value => _isSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value on a failed result.");

    public ErrorInfo Error => !_isSuccess
        ? _error!
        : throw new InvalidOperationException("Cannot access Error on a successful result.");

    public static Result<T> Success(T value) => new(value);

    public static Result<T> Failure(ErrorInfo error) => new(error);

    public static implicit operator Result<T>(T value) => new(value);

    public static implicit operator Result<T>(ErrorInfo error) => new(error);

    public Result<U> Map<U>(Func<T, U> transform) =>
        _isSuccess ? Result<U>.Success(transform(_value!)) : Result<U>.Failure(_error!);

    public Result<U> Bind<U>(Func<T, Result<U>> transform) =>
        _isSuccess ? transform(_value!) : Result<U>.Failure(_error!);

    public U Match<U>(Func<T, U> onSuccess, Func<ErrorInfo, U> onFailure) =>
        _isSuccess ? onSuccess(_value!) : onFailure(_error!);
}
