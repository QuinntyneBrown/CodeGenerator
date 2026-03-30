// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Detox.Syntax;

public class JestConfigModel : SyntaxModel
{
    public JestConfigModel()
    {
        TestMatch = "<rootDir>/specs/**/*.spec.ts";
        TestTimeout = 120000;
        SetupTimeout = 120000;
    }

    public string TestMatch { get; set; }

    public int TestTimeout { get; set; }

    public int SetupTimeout { get; set; }
}
