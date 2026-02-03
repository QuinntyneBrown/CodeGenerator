// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO.Abstractions;
using CodeGenerator.DotNet.Artifacts.Files;
using CodeGenerator.DotNet.Artifacts.Projects.Enums;
using CodeGenerator.DotNet.Syntax.Classes;

namespace CodeGenerator.DotNet.Artifacts.Projects;

public class ConsoleMicroserviceTestProjectModel : ProjectModel
{
    public ConsoleMicroserviceTestProjectModel(string name, string directory, IFileSystem? fileSystem = null)
    {
        fileSystem ??= new FileSystem();
        Name = name;
        Directory = $"{directory}{fileSystem.Path.DirectorySeparatorChar}{name}";
        DotNetProjectType = DotNetProjectType.XUnit;
        Packages.Add(new PackageModel("Moq", "4.18.4"));
        Order = 1;
        References = new List<string>
        {
            @$"..\\{name.Replace(".Tests", string.Empty)}\\{name.Replace(".Tests", string.Empty)}.csproj",
        };

        var classModel = new ClassModel("Worker");

        var classFileModel = new CodeFileModel<ClassModel>(classModel, classModel.Usings, "Worker", Directory, ".cs", fileSystem);

        Files = new List<FileModel>
        {
            classFileModel,
        };
    }
}
