// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.DotNet.Artifacts.Files;

public class LaunchSettingsFileModel : FileModel
{
    public LaunchSettingsFileModel(string directory)
        : base("launchSettings", directory, ".json")
    {
    }
}
