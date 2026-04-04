// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.IO;

public static class Retry
{
    public static async Task<T> ExecuteAsync<T>(
        Func<Task<T>> action,
        RetryOptions? options = null,
        CancellationToken ct = default)
    {
        options ??= new RetryOptions();
        Exception? lastException = null;
        var delay = options.InitialDelay;

        for (int attempt = 1; attempt <= options.MaxAttempts; attempt++)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                return await action().ConfigureAwait(false);
            }
            catch (Exception ex) when (attempt < options.MaxAttempts && (options.ShouldRetry?.Invoke(ex) ?? IsTransient(ex)))
            {
                lastException = ex;
                var actualDelay = delay < options.MaxDelay ? delay : options.MaxDelay;
                await Task.Delay(actualDelay, ct).ConfigureAwait(false);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * options.BackoffMultiplier);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        throw lastException!;
    }

    public static async Task ExecuteAsync(
        Func<Task> action,
        RetryOptions? options = null,
        CancellationToken ct = default)
    {
        await ExecuteAsync(async () =>
        {
            await action().ConfigureAwait(false);
            return 0;
        }, options, ct).ConfigureAwait(false);
    }

    public static bool IsTransient(Exception ex)
    {
        return ex is IOException && ex is not FileNotFoundException && ex is not DirectoryNotFoundException;
    }
}
