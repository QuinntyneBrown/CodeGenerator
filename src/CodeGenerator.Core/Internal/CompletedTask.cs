// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Internal;

public static class CompletedTask
{
    public static readonly Task Instance = Task.CompletedTask;
}