// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;

namespace CodeGenerator.DotNet.Artifacts.Files;

public class CodeFileModel<T> : FileModel
    where T : SyntaxModel
{
    public CodeFileModel()
        : base(string.Empty, string.Empty, string.Empty)
    {
    }

    public CodeFileModel(T @object, string name, string directory, string extension, IFileSystem? fileSystem = null)
    : base(name.Split('.').Last(), directory, extension, fileSystem)
    {
        Object = @object;
        Usings = @object.Usings;
    }

    public CodeFileModel(T @object, List<UsingModel> usings, string name, string directory, string extension, IFileSystem? fileSystem = null)
        : base(name.Split('.').Last(), directory, extension, fileSystem)
    {
        Object = @object;
        Usings = usings;
    }

    public T Object { get; set; }

    public List<UsingModel> Usings { get; set; }

    public string Namespace { get; set; }

    public string AdditionalCode { get; set; }
}
