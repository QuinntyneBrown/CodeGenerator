// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Services;

public static class ResourceId
{
    public static string For<T>(string name) => $"{typeof(T).Name}:{name}";
    public static string For(object model, string name) => $"{model.GetType().Name}:{name}";
}
