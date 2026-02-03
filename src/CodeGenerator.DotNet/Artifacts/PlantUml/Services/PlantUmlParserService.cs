// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CodeGenerator.DotNet.Artifacts.PlantUml.Models;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Artifacts.PlantUml.Services;

public class PlantUmlParserService : IPlantUmlParserService
{
    private readonly ILogger<PlantUmlParserService> logger;
    private readonly IFileSystem fileSystem;

    private static readonly Regex ClassRegex = new(
        @"class\s+(\w+)\s*(?:<<(\w+)>>)?\s*\{([^}]*)\}",
        RegexOptions.Compiled | RegexOptions.Singleline);

    private static readonly Regex EnumRegex = new(
        @"enum\s+(\w+)\s*\{([^}]*)\}",
        RegexOptions.Compiled | RegexOptions.Singleline);

    private static readonly Regex PropertyRegex = new(
        @"^([+\-#~])?\s*(\w+)\s*:\s*(\w+(?:<[\w,\s]+>)?)\s*(\?)?(?:\s*=\s*(.+))?$",
        RegexOptions.Compiled | RegexOptions.Multiline);

    private static readonly Regex MethodRegex = new(
        @"^([+\-#~])?\s*(\w+)\s*\(([^)]*)\)\s*:\s*(\w+(?:<[\w,\s]+>)?)$",
        RegexOptions.Compiled | RegexOptions.Multiline);

    private static readonly Regex RelationshipRegex = new(
        @"(\w+)\s*""([^""]+)""\s*([*o\-\.]+)\s*""([^""]+)""\s*(\w+)\s*:\s*(.+)",
        RegexOptions.Compiled);

    private static readonly Regex SimpleRelationshipRegex = new(
        @"(\w+)\s*([*o\-\.]+>?|<[*o\-\.]+)\s*(\w+)(?:\s*:\s*(.+))?",
        RegexOptions.Compiled);

    private static readonly Regex PackageRegex = new(
        @"package\s+""([^""]+)""\s*(?:<<(\w+)>>)?\s*\{((?:[^{}]|(?<open>\{)|(?<-open>\}))*(?(open)(?!)))\}",
        RegexOptions.Compiled | RegexOptions.Singleline);

    private static readonly Regex ComponentRegex = new(
        @"component\s+\[([^\]]+)\]\s+as\s+(\w+)\s*(?:<<(\w+)>>)?",
        RegexOptions.Compiled);

    private static readonly Regex NoteRegex = new(
        @"note\s+(?:right|left|top|bottom)?\s*(?:of\s+(\w+))?\s*\n((?:[^\n]|\n(?!end note))*)\nend note",
        RegexOptions.Compiled | RegexOptions.Singleline);

    private static readonly Regex TitleRegex = new(
        @"title\s+(.+)",
        RegexOptions.Compiled);

    private static readonly Regex GenericTypeRegex = new(
        @"(\w+)<([\w,\s]+)>",
        RegexOptions.Compiled);

    private static readonly Regex ParticipantRegex = new(
        @"(?:participant|actor)\s+(?:""([^""]+)""|(\w+))\s+as\s+(\w+)",
        RegexOptions.Compiled | RegexOptions.Multiline);

    public PlantUmlParserService(ILogger<PlantUmlParserService> logger, IFileSystem fileSystem)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public async Task<PlantUmlSolutionModel> ParseDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Parsing PlantUML files from directory: {Directory}", directoryPath);

        var result = new PlantUmlSolutionModel
        {
            SourcePath = directoryPath,
            Name = Path.GetFileName(directoryPath)
        };

        if (!fileSystem.Directory.Exists(directoryPath))
        {
            logger.LogWarning("Directory does not exist: {Directory}", directoryPath);
            return result;
        }

        var pumlFiles = fileSystem.Directory.GetFiles(directoryPath, "*.puml", SearchOption.AllDirectories);

        foreach (var file in pumlFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var document = await ParseFileAsync(file, cancellationToken);
                if (document != null)
                {
                    result.Documents.Add(document);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to parse PlantUML file: {File}", file);
            }
        }

        logger.LogInformation("Parsed {Count} PlantUML documents", result.Documents.Count);
        return result;
    }

    public async Task<PlantUmlDocumentModel> ParseFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Parsing PlantUML file: {File}", filePath);

        if (!fileSystem.File.Exists(filePath))
        {
            logger.LogWarning("File does not exist: {File}", filePath);
            return null;
        }

        var content = await Task.Run(() => fileSystem.File.ReadAllText(filePath), cancellationToken);
        return ParseContent(content, filePath);
    }

    public PlantUmlDocumentModel ParseContent(string content, string sourcePath = null)
    {
        var document = new PlantUmlDocumentModel
        {
            FilePath = sourcePath
        };

        // Extract title
        var titleMatch = TitleRegex.Match(content);
        if (titleMatch.Success)
        {
            document.Title = titleMatch.Groups[1].Value.Trim();
        }

        // Parse packages first
        ParsePackages(content, document);

        // Parse classes outside packages
        ParseClasses(content, document);

        // Parse enums outside packages
        ParseEnums(content, document);

        // Parse relationships
        ParseRelationships(content, document);

        // Parse components
        ParseComponents(content, document);

        // Parse participants (for sequence diagrams)
        ParseParticipants(content, document);

        // Parse notes and associate with elements
        ParseNotes(content, document);

        // Parse metadata if present
        ParseMetadata(content, document);

        return document;
    }

    private void ParsePackages(string content, PlantUmlDocumentModel document)
    {
        var packageMatches = PackageRegex.Matches(content);

        foreach (Match match in packageMatches)
        {
            var package = new PlantUmlPackageModel
            {
                Name = match.Groups[1].Value.Trim(),
                Stereotype = match.Groups[2].Success ? match.Groups[2].Value.Trim() : null
            };

            var packageContent = match.Groups[3].Value;

            // Parse classes within package
            var classMatches = ClassRegex.Matches(packageContent);
            foreach (Match classMatch in classMatches)
            {
                var classModel = ParseClassMatch(classMatch, package.Name);
                package.Classes.Add(classModel);
            }

            // Parse enums within package
            var enumMatches = EnumRegex.Matches(packageContent);
            foreach (Match enumMatch in enumMatches)
            {
                var enumModel = ParseEnumMatch(enumMatch, package.Name);
                package.Enums.Add(enumModel);
            }

            document.Packages.Add(package);
        }
    }

    private void ParseClasses(string content, PlantUmlDocumentModel document)
    {
        // Remove package content to avoid double parsing
        var contentWithoutPackages = PackageRegex.Replace(content, string.Empty);

        var classMatches = ClassRegex.Matches(contentWithoutPackages);
        foreach (Match match in classMatches)
        {
            var classModel = ParseClassMatch(match, null);
            document.Classes.Add(classModel);
        }
    }

    private PlantUmlClassModel ParseClassMatch(Match match, string defaultNamespace)
    {
        var className = match.Groups[1].Value.Trim();
        var stereotypeStr = match.Groups[2].Success ? match.Groups[2].Value.Trim().ToLower() : null;
        var classBody = match.Groups[3].Value;

        var classModel = new PlantUmlClassModel
        {
            Name = className,
            Namespace = defaultNamespace,
            BoundedContext = ExtractBoundedContext(defaultNamespace),
            Stereotype = ParseStereotype(stereotypeStr)
        };

        // Parse properties
        var lines = classBody.Split('\n');
        var isMethodSection = false;

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("'"))
            {
                continue;
            }

            if (trimmedLine == "__")
            {
                isMethodSection = true;
                continue;
            }

            if (isMethodSection)
            {
                var methodMatch = MethodRegex.Match(trimmedLine);
                if (methodMatch.Success)
                {
                    classModel.Methods.Add(ParseMethodMatch(methodMatch));
                }
            }
            else
            {
                var propertyMatch = PropertyRegex.Match(trimmedLine);
                if (propertyMatch.Success)
                {
                    classModel.Properties.Add(ParsePropertyMatch(propertyMatch));
                }
            }
        }

        // Post-process: correctly identify the primary key
        // Only {ClassName}Id should be treated as the primary key, not foreign keys like UserId, CustomerId, etc.
        var expectedPrimaryKeyName = $"{className}Id";
        foreach (var property in classModel.Properties)
        {
            property.IsKey = property.Name.Equals(expectedPrimaryKeyName, StringComparison.OrdinalIgnoreCase);
        }

        return classModel;
    }

    private PlantUmlPropertyModel ParsePropertyMatch(Match match)
    {
        var visibility = ParseVisibility(match.Groups[1].Value);
        var name = match.Groups[2].Value.Trim();
        var typeStr = match.Groups[3].Value.Trim();
        var isNullable = match.Groups[4].Success;
        var defaultValue = match.Groups[5].Success ? match.Groups[5].Value.Trim() : null;

        var property = new PlantUmlPropertyModel
        {
            Name = name,
            Visibility = visibility,
            IsNullable = isNullable,
            DefaultValue = defaultValue
        };

        // Parse generic types (e.g., List<Comment>)
        var genericMatch = GenericTypeRegex.Match(typeStr);
        if (genericMatch.Success)
        {
            property.CollectionType = genericMatch.Groups[1].Value.Trim();
            property.GenericTypeArgument = genericMatch.Groups[2].Value.Trim();
            property.Type = property.GenericTypeArgument;
            property.IsCollection = IsCollectionType(property.CollectionType);
        }
        else
        {
            property.Type = typeStr;
        }

        // IsKey will be set correctly in ParseClassMatch post-processing
        // based on whether it matches {ClassName}Id pattern
        property.IsKey = false;

        // Detect if required based on nullable
        property.IsRequired = !isNullable;

        return property;
    }

    private PlantUmlMethodModel ParseMethodMatch(Match match)
    {
        var visibility = ParseVisibility(match.Groups[1].Value);
        var name = match.Groups[2].Value.Trim();
        var parameters = match.Groups[3].Value.Trim();
        var returnType = match.Groups[4].Value.Trim();

        var method = new PlantUmlMethodModel
        {
            Name = name,
            ReturnType = returnType,
            Visibility = visibility
        };

        // Parse parameters
        if (!string.IsNullOrWhiteSpace(parameters))
        {
            var paramParts = parameters.Split(',');
            foreach (var param in paramParts)
            {
                var paramTrimmed = param.Trim();
                var colonIndex = paramTrimmed.IndexOf(':');
                if (colonIndex > 0)
                {
                    method.Parameters.Add(new PlantUmlParameterModel
                    {
                        Name = paramTrimmed[..colonIndex].Trim(),
                        Type = paramTrimmed[(colonIndex + 1)..].Trim()
                    });
                }
            }
        }

        return method;
    }

    private void ParseEnums(string content, PlantUmlDocumentModel document)
    {
        // Remove package content to avoid double parsing
        var contentWithoutPackages = PackageRegex.Replace(content, string.Empty);

        var enumMatches = EnumRegex.Matches(contentWithoutPackages);
        foreach (Match match in enumMatches)
        {
            var enumModel = ParseEnumMatch(match, null);
            document.Enums.Add(enumModel);
        }
    }

    private PlantUmlEnumModel ParseEnumMatch(Match match, string defaultNamespace)
    {
        var enumName = match.Groups[1].Value.Trim();
        var enumBody = match.Groups[2].Value;

        var enumModel = new PlantUmlEnumModel
        {
            Name = enumName,
            Namespace = defaultNamespace,
            BoundedContext = ExtractBoundedContext(defaultNamespace)
        };

        var lines = enumBody.Split('\n');
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (!string.IsNullOrWhiteSpace(trimmedLine) && !trimmedLine.StartsWith("'"))
            {
                enumModel.Values.Add(trimmedLine.TrimEnd(','));
            }
        }

        return enumModel;
    }

    private void ParseRelationships(string content, PlantUmlDocumentModel document)
    {
        // Parse relationships with cardinality
        var relationshipMatches = RelationshipRegex.Matches(content);
        foreach (Match match in relationshipMatches)
        {
            document.Relationships.Add(ParseRelationshipMatch(match));
        }

        // Parse simple relationships
        var simpleMatches = SimpleRelationshipRegex.Matches(content);
        foreach (Match match in simpleMatches)
        {
            // Skip if already parsed by the more specific regex
            var fullMatch = match.Value;
            if (!relationshipMatches.Cast<Match>().Any(m => m.Value.Contains(fullMatch)))
            {
                var relationship = ParseSimpleRelationshipMatch(match);
                if (relationship != null)
                {
                    document.Relationships.Add(relationship);
                }
            }
        }
    }

    private PlantUmlRelationshipModel ParseRelationshipMatch(Match match)
    {
        var source = match.Groups[1].Value.Trim();
        var sourceCardinality = match.Groups[2].Value.Trim();
        var relationshipSymbol = match.Groups[3].Value.Trim();
        var targetCardinality = match.Groups[4].Value.Trim();
        var target = match.Groups[5].Value.Trim();
        var label = match.Groups[6].Value.Trim();

        return new PlantUmlRelationshipModel
        {
            SourceClass = source,
            TargetClass = target,
            SourceCardinality = sourceCardinality,
            TargetCardinality = targetCardinality,
            RelationshipType = ParseRelationshipType(relationshipSymbol),
            Label = label
        };
    }

    private PlantUmlRelationshipModel ParseSimpleRelationshipMatch(Match match)
    {
        var source = match.Groups[1].Value.Trim();
        var relationshipSymbol = match.Groups[2].Value.Trim();
        var target = match.Groups[3].Value.Trim();
        var label = match.Groups[4].Success ? match.Groups[4].Value.Trim() : null;

        // Skip if source or target look like keywords or operators
        if (IsKeyword(source) || IsKeyword(target))
        {
            return null;
        }

        return new PlantUmlRelationshipModel
        {
            SourceClass = source,
            TargetClass = target,
            RelationshipType = ParseRelationshipType(relationshipSymbol),
            Label = label
        };
    }

    private void ParseComponents(string content, PlantUmlDocumentModel document)
    {
        var componentMatches = ComponentRegex.Matches(content);
        foreach (Match match in componentMatches)
        {
            var component = new PlantUmlComponentModel
            {
                Name = match.Groups[1].Value.Trim(),
                Alias = match.Groups[2].Value.Trim(),
                Stereotype = match.Groups[3].Success ? match.Groups[3].Value.Trim() : null
            };

            document.Components.Add(component);
        }
    }

    private void ParseParticipants(string content, PlantUmlDocumentModel document)
    {
        var participantMatches = ParticipantRegex.Matches(content);
        var lines = content.Split('\n');
        
        foreach (Match match in participantMatches)
        {
            var participant = new PlantUmlParticipantModel
            {
                // Group 1 is quoted name, Group 2 is unquoted name, Group 3 is alias
                Name = match.Groups[1].Success ? match.Groups[1].Value.Trim() : match.Groups[2].Value.Trim(),
                Alias = match.Groups[3].Value.Trim()
            };

            // Look for a comment line immediately before this participant
            // to get the DotNetType (Angular, Worker, Microservice)
            var matchLineIndex = FindLineIndex(lines, match.Index);
            if (matchLineIndex > 0)
            {
                var previousLine = lines[matchLineIndex - 1].Trim();
                if (previousLine.StartsWith("'") || previousLine.StartsWith("//"))
                {
                    // Extract the type from the comment
                    var commentContent = previousLine.TrimStart('\'', '/', ' ').Trim();
                    participant.DotNetType = commentContent;
                }
            }

            // Determine the type (participant or actor) from the match
            var lineText = GetLineAtIndex(lines, match.Index);
            if (lineText.TrimStart().StartsWith("actor"))
            {
                participant.Type = "actor";
            }
            else if (lineText.TrimStart().StartsWith("participant"))
            {
                participant.Type = "participant";
            }

            document.Participants.Add(participant);
        }
    }

    private int FindLineIndex(string[] lines, int charIndex)
    {
        int currentIndex = 0;
        for (int i = 0; i < lines.Length; i++)
        {
            currentIndex += lines[i].Length + 1; // +1 for newline
            if (currentIndex > charIndex)
            {
                return i;
            }
        }
        return lines.Length - 1;
    }

    private string GetLineAtIndex(string[] lines, int charIndex)
    {
        int lineIndex = FindLineIndex(lines, charIndex);
        return lineIndex >= 0 && lineIndex < lines.Length ? lines[lineIndex] : string.Empty;
    }

    private void ParseNotes(string content, PlantUmlDocumentModel document)
    {
        var noteMatches = NoteRegex.Matches(content);
        foreach (Match match in noteMatches)
        {
            var targetAlias = match.Groups[1].Success ? match.Groups[1].Value.Trim() : null;
            var noteContent = match.Groups[2].Value.Trim();

            if (!string.IsNullOrEmpty(targetAlias))
            {
                // Try to associate with a class
                var targetClass = document.Classes.FirstOrDefault(c => c.Name == targetAlias) ??
                                 document.Packages.SelectMany(p => p.Classes).FirstOrDefault(c => c.Name == targetAlias);

                if (targetClass != null)
                {
                    targetClass.Note = noteContent;
                    continue;
                }

                // Try to associate with a component
                var targetComponent = document.Components.FirstOrDefault(c => c.Alias == targetAlias || c.Name == targetAlias);
                if (targetComponent != null)
                {
                    targetComponent.Note = noteContent;
                    ParseCodeGeneratorSpecFromNote(noteContent, targetComponent);
                }
            }
        }
    }

    private void ParseCodeGeneratorSpecFromNote(string noteContent, PlantUmlComponentModel component)
    {
        var spec = new PlantUmlCodeGeneratorSpecification();

        // Parse route
        var routeMatch = Regex.Match(noteContent, @"\*\*Route:\*\*\s*([^\n]+)");
        if (routeMatch.Success)
        {
            spec.Route = routeMatch.Groups[1].Value.Trim();
        }

        // Parse authentication
        var authMatch = Regex.Match(noteContent, @"\*\*Authentication:\*\*\s*(\w+)");
        if (authMatch.Success)
        {
            spec.AuthenticationRequired = authMatch.Groups[1].Value.Trim().Equals("Required", StringComparison.OrdinalIgnoreCase);
        }

        // Parse endpoints
        var endpointPattern = @"(GET|POST|PUT|DELETE|PATCH)\s+([^\s-]+)\s*-?\s*([^\n]+)?";
        var endpointMatches = Regex.Matches(noteContent, endpointPattern);
        foreach (Match match in endpointMatches)
        {
            var endpoint = new PlantUmlCodeGeneratorModel
            {
                HttpVerb = match.Groups[1].Value.Trim(),
                Path = match.Groups[2].Value.Trim(),
                Description = match.Groups[3].Success ? match.Groups[3].Value.Trim() : null
            };

            spec.CodeGenerators.Add(endpoint);
        }

        if (spec.CodeGenerators.Count > 0 || !string.IsNullOrEmpty(spec.Route))
        {
            component.CodeGeneratorSpec = spec;
        }
    }

    private void ParseMetadata(string content, PlantUmlDocumentModel document)
    {
        var metadataMatch = Regex.Match(content, @"note\s+""Document Metadata""\s+as\s+\w+\s*\n((?:[^\n]|\n(?!end note))*)\nend note", RegexOptions.Singleline);
        if (metadataMatch.Success)
        {
            var metadataContent = metadataMatch.Groups[1].Value;
            document.Metadata = new PlantUmlMetadataModel();

            var generatedMatch = Regex.Match(metadataContent, @"\*\*Generated:\*\*\s*([^\n]+)");
            if (generatedMatch.Success)
            {
                document.Metadata.GeneratedDate = generatedMatch.Groups[1].Value.Trim();
            }

            var versionMatch = Regex.Match(metadataContent, @"\*\*Version:\*\*\s*([^\n]+)");
            if (versionMatch.Success)
            {
                document.Metadata.Version = versionMatch.Groups[1].Value.Trim();
            }

            var sourceMatch = Regex.Match(metadataContent, @"\*\*Source:\*\*\s*([^\n]+)");
            if (sourceMatch.Success)
            {
                document.Metadata.Source = sourceMatch.Groups[1].Value.Trim();
            }

            var frameworkMatch = Regex.Match(metadataContent, @"\*\*Target Framework:\*\*\s*([^\n]+)");
            if (frameworkMatch.Success)
            {
                document.Metadata.TargetFramework = frameworkMatch.Groups[1].Value.Trim();
            }

            var angularMatch = Regex.Match(metadataContent, @"\*\*Angular Version:\*\*\s*([^\n]+)");
            if (angularMatch.Success)
            {
                document.Metadata.AngularVersion = angularMatch.Groups[1].Value.Trim();
            }
        }
    }

    private static PlantUmlStereotype ParseStereotype(string stereotype)
    {
        if (string.IsNullOrEmpty(stereotype))
        {
            return PlantUmlStereotype.None;
        }

        return stereotype.ToLower() switch
        {
            "aggregate" => PlantUmlStereotype.Aggregate,
            "entity" => PlantUmlStereotype.Entity,
            "valueobject" => PlantUmlStereotype.ValueObject,
            "enum" => PlantUmlStereotype.Enum,
            "service" => PlantUmlStereotype.Service,
            _ => PlantUmlStereotype.None
        };
    }

    private static PlantUmlVisibility ParseVisibility(string symbol)
    {
        return symbol switch
        {
            "+" => PlantUmlVisibility.Public,
            "-" => PlantUmlVisibility.Private,
            "#" => PlantUmlVisibility.Protected,
            "~" => PlantUmlVisibility.Package,
            _ => PlantUmlVisibility.Public
        };
    }

    private static PlantUmlRelationshipType ParseRelationshipType(string symbol)
    {
        if (symbol.Contains("*--"))
        {
            return PlantUmlRelationshipType.Composition;
        }

        if (symbol.Contains("o--"))
        {
            return PlantUmlRelationshipType.Aggregation;
        }

        if (symbol.Contains("..|>"))
        {
            return PlantUmlRelationshipType.Implementation;
        }

        if (symbol.Contains("--|>"))
        {
            return PlantUmlRelationshipType.Inheritance;
        }

        if (symbol.Contains(".."))
        {
            return PlantUmlRelationshipType.Dependency;
        }

        return PlantUmlRelationshipType.Association;
    }

    private static bool IsCollectionType(string typeName)
    {
        var collectionTypes = new[] { "List", "IList", "ICollection", "IEnumerable", "Collection", "HashSet", "ISet" };
        return collectionTypes.Any(t => t.Equals(typeName, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsKeyword(string word)
    {
        var keywords = new[] { "class", "enum", "component", "package", "note", "end", "abstract", "interface" };
        return keywords.Any(k => k.Equals(word, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Extracts the bounded context name from a namespace.
    /// Expected format: {SolutionName}.{BoundedContext}.Aggregates.{EntityName}
    /// Returns null if no bounded context is detected.
    /// </summary>
    private static string ExtractBoundedContext(string namespaceStr)
    {
        if (string.IsNullOrEmpty(namespaceStr))
        {
            return null;
        }

        var parts = namespaceStr.Split('.');

        // Look for the pattern: {SolutionName}.{BoundedContext}.Aggregates.{EntityName}
        // The bounded context is the segment immediately before "Aggregates"
        var aggregatesIndex = Array.FindIndex(parts, p => p.Equals("Aggregates", StringComparison.OrdinalIgnoreCase));

        if (aggregatesIndex > 1)
        {
            // There's at least one segment before "Aggregates" after the solution name
            // e.g., ["ECommerce", "OrderManagement", "Aggregates", "Order"] -> "OrderManagement"
            var boundedContext = parts[aggregatesIndex - 1];

            // Skip if the segment looks like "Core" (default context)
            if (!boundedContext.Equals("Core", StringComparison.OrdinalIgnoreCase))
            {
                return boundedContext;
            }
        }

        return null;
    }
}
