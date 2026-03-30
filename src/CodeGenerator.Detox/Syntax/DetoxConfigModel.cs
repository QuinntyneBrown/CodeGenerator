// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Detox.Syntax;

public class DetoxConfigModel : SyntaxModel
{
    public DetoxConfigModel(string appName)
    {
        AppName = appName;
        IosBuild = string.Empty;
        AndroidBuild = string.Empty;
        TestRunner = "jest";
    }

    public string AppName { get; set; }

    public string IosBuild { get; set; }

    public string AndroidBuild { get; set; }

    public string TestRunner { get; set; }
}
