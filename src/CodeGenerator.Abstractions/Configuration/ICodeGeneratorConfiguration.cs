// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Configuration;

public interface ICodeGeneratorConfiguration
{
    string? GetValue(string key);
    T GetValue<T>(string key, T defaultValue = default!);
    bool HasKey(string key);
    IReadOnlyDictionary<string, string> GetAll();
    IReadOnlyDictionary<string, string> GetSection(string prefix);
}
