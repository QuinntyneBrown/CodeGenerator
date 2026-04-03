// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Templates;

public interface IFilenamePlaceholderResolver
{
    FilenamePlaceholderResult Analyze(string filename);
    string Resolve(string filename, IDictionary<string, object> tokens);
}
