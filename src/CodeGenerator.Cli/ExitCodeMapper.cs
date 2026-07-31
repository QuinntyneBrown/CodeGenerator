// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Cli.Formatting;
using CodeGenerator.Core.Errors;

namespace CodeGenerator.Cli;

/// <summary>
/// Maps an exception escaping a command handler onto the process exit code and writes the
/// formatted failure to the error stream.
/// </summary>
/// <remarks>
/// This is installed as the <c>System.CommandLine</c> exception-handler middleware.
/// The library's default handler catches every handler exception, writes a raw stack
/// trace, and returns exit code 1 — which would flatten the whole
/// <see cref="CliExitCodes"/> taxonomy to a single value. Routing through this type is
/// what makes the documented exit codes observable.
/// </remarks>
public static class ExitCodeMapper
{
    public static int Map(Exception exception, bool verbose, IErrorFormatter formatter, TextWriter error)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentNullException.ThrowIfNull(error);

        switch (exception)
        {
            case CliAggregateException aggregate:
                foreach (var inner in aggregate.InnerExceptions)
                {
                    error.WriteLine(inner is CliException cliInner
                        ? formatter.FormatException(cliInner, verbose)
                        : $"ERROR [{ErrorCodes.InternalUnexpected}] {inner.Message}");
                }

                return aggregate.ExitCode;

            case CliValidationException validation when validation.ValidationResult is not null:
                error.Write(formatter.FormatValidationResult(validation.ValidationResult));
                return validation.ExitCode;

            case CliException cli:
                error.WriteLine(formatter.FormatException(cli, verbose));
                return cli.ExitCode;

            case OperationCanceledException:
                error.WriteLine("Operation cancelled.");
                return CliExitCodes.Cancelled;

            default:
                error.WriteLine($"ERROR [{ErrorCodes.InternalUnexpected}] An unexpected error occurred.");

                if (verbose)
                {
                    error.WriteLine(exception.ToString());
                }
                else
                {
                    error.WriteLine("Re-run with --verbose to see the full stack trace.");
                }

                return CliExitCodes.UnexpectedError;
        }
    }
}
