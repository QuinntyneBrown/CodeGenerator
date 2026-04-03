// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Services;

public interface IGenerationContext
{
    void Push(string stackName, object value);
    IReadOnlyList<object> GetStack(string stackName);
    void Set(string key, object value);
    T Get<T>(string key);
    bool TryGet<T>(string key, out T value);
    IReadOnlyList<GeneratedFileRecord> GeneratedFiles { get; }
    void RecordGeneratedFile(string path, string generatorName);

    void MarkHandled(string resourceId, string handlerName);
    bool IsHandled(string resourceId);
    string GetHandler(string resourceId);
    IReadOnlyDictionary<string, string> GetAllHandled();
}
