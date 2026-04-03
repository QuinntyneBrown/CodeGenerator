// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Services;
using DotLiquid;
using Humanizer;

namespace CodeGenerator.Core.Liquid;

public static class CodeGeneratorFilters
{
    private static INamingConventionConverter? _converter;

    public static void Initialize(INamingConventionConverter converter)
    {
        _converter = converter;
    }

    // --- Naming convention filters ---

    public static string Pascal(string input)
        => _converter!.Convert(NamingConvention.PascalCase, input);

    public static string Camel(string input)
        => _converter!.Convert(NamingConvention.CamelCase, input);

    public static string Snake(string input)
        => _converter!.Convert(NamingConvention.KebobCase, input);

    public static string Kebab(string input)
        => _converter!.Convert(NamingConvention.SnakeCase, input);

    public static string Title(string input)
        => _converter!.Convert(NamingConvention.TitleCase, input);

    public static string Allcaps(string input)
        => _converter!.Convert(NamingConvention.AllCaps, input);

    // --- String manipulation filters ---

    public static string Namespace(string input)
    {
        var lastDot = input.LastIndexOf('.');
        return lastDot > 0 ? input[..lastDot] : string.Empty;
    }

    public static string Strip_namespace(string input)
    {
        var lastDot = input.LastIndexOf('.');
        return lastDot >= 0 ? input[(lastDot + 1)..] : input;
    }

    // --- Pluralization filters ---

    public static string Pluralize(string input)
        => InflectorExtensions.Pluralize(input, inputIsKnownToBeSingular: false);

    public static string Singularize(string input)
        => InflectorExtensions.Singularize(input, inputIsKnownToBePlural: false);

    // --- Type mapping filter ---

    public static string Schema_type(DotLiquid.Context context, string input)
    {
        var language = context["language"]?.ToString() ?? "csharp";
        return TypeMapper.Map(input, language);
    }
}
