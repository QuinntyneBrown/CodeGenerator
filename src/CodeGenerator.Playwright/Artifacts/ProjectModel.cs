// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts;

namespace CodeGenerator.Playwright.Artifacts;

public class ProjectModel : ArtifactModel
{
    public ProjectModel(string name, string directory, string baseUrl, List<string>? browsers = null)
    {
        Name = name;
        Directory = $"{directory}{Path.DirectorySeparatorChar}{name}";
        RootDirectory = directory;
        BaseUrl = baseUrl;
        Browsers = browsers ?? ["chromium", "firefox", "webkit"];
    }

    public string Name { get; set; }

    public string Directory { get; set; }

    public string RootDirectory { get; set; }

    public string BaseUrl { get; set; }

    public List<string> Browsers { get; set; }
}
