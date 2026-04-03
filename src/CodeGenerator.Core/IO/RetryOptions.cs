// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.IO;

public class RetryOptions
{
    public int MaxAttempts { get; set; } = 3;
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromMilliseconds(100);
    public double BackoffMultiplier { get; set; } = 2.0;
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(5);

    public static RetryOptions FileWrite => new()
    {
        MaxAttempts = 3,
        InitialDelay = TimeSpan.FromMilliseconds(100),
        BackoffMultiplier = 2.0,
        MaxDelay = TimeSpan.FromSeconds(5)
    };

    public static RetryOptions DirectoryCreate => new()
    {
        MaxAttempts = 3,
        InitialDelay = TimeSpan.FromMilliseconds(50),
        BackoffMultiplier = 2.0,
        MaxDelay = TimeSpan.FromSeconds(2)
    };

    public static RetryOptions TemplateLoad => new()
    {
        MaxAttempts = 4,
        InitialDelay = TimeSpan.FromMilliseconds(200),
        BackoffMultiplier = 2.0,
        MaxDelay = TimeSpan.FromSeconds(10)
    };

    public static RetryOptions None => new()
    {
        MaxAttempts = 1,
        InitialDelay = TimeSpan.Zero,
        BackoffMultiplier = 1.0,
        MaxDelay = TimeSpan.Zero
    };
}
