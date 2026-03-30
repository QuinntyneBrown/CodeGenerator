// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Python;

public static class Constants
{
    public static class FileExtensions
    {
        public static string Python = ".py";
        public static string Requirements = ".txt";
        public static string Toml = ".toml";
        public static string Cfg = ".cfg";
        public static string Ini = ".ini";
    }

    public static class ProjectType
    {
        public const string Flask = nameof(Flask);
        public const string Django = nameof(Django);
        public const string Package = nameof(Package);
        public const string Script = nameof(Script);
    }

    public static class TemplateTypes
    {
        public const string InitFile = nameof(InitFile);
        public const string SetupPy = nameof(SetupPy);
        public const string SetupCfg = nameof(SetupCfg);
        public const string PyProjectToml = nameof(PyProjectToml);
        public const string ManagePy = nameof(ManagePy);
        public const string AppPy = nameof(AppPy);
    }

    public static class FileNames
    {
        public const string Init = "__init__";
        public const string Main = "__main__";
        public const string Requirements = "requirements";
        public const string SetupPy = "setup";
        public const string Conftest = "conftest";
    }
}
