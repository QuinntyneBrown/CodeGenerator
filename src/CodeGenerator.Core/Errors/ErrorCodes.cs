// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Errors;

public static class ErrorCodes
{
    public const string InternalUnexpected = "INTERNAL_UNEXPECTED";

    public static class Validation
    {
        public const string InvalidIdentifier = "VALIDATION_INVALID_IDENTIFIER";
        public const string MissingRequired = "VALIDATION_MISSING_REQUIRED";
        public const string DirNotWritable = "VALIDATION_DIR_NOT_WRITABLE";
    }

    public static class Io
    {
        public const string FileNotFound = "IO_FILE_NOT_FOUND";
        public const string AccessDenied = "IO_ACCESS_DENIED";
        public const string DirCreateFailed = "IO_DIR_CREATE_FAILED";
    }

    public static class Template
    {
        public const string NotFound = "TEMPLATE_NOT_FOUND";
        public const string RenderFailed = "TEMPLATE_RENDER_FAILED";
        public const string SyntaxError = "TEMPLATE_SYNTAX_ERROR";
    }

    public static class Scaffold
    {
        public const string ParseFailed = "SCAFFOLD_PARSE_FAILED";
        public const string FileConflict = "SCAFFOLD_FILE_CONFLICT";
        public const string PostCmdFailed = "SCAFFOLD_POST_CMD_FAILED";
    }

    public static class Process
    {
        public const string Timeout = "PROCESS_TIMEOUT";
        public const string NonZeroExit = "PROCESS_NON_ZERO_EXIT";
    }

    public static class Plugin
    {
        public const string LoadFailed = "PLUGIN_LOAD_FAILED";
        public const string StrategyNotFound = "PLUGIN_STRATEGY_NOT_FOUND";
    }

    public static class Schema
    {
        public const string Invalid = "SCHEMA_INVALID";
        public const string EntityInvalid = "SCHEMA_ENTITY_INVALID";
    }

    public static class Configuration
    {
        public const string Missing = "CONFIG_MISSING";
        public const string Invalid = "CONFIG_INVALID";
    }
}
