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

/// <summary>
/// Implementation of the PlantUML validation service that validates files against the PlantUML scaffolding specification.
/// </summary>
public class PlantUmlValidationService : IPlantUmlValidationService
{
    private readonly ILogger<PlantUmlValidationService> logger;
    private readonly IPlantUmlParserService parserService;
    private readonly IFileSystem fileSystem;

    /// <summary>
    /// Required document files as specified in the PlantUML scaffolding specification.
    /// </summary>
    private static readonly HashSet<string> RequiredDocuments = new(StringComparer.OrdinalIgnoreCase)
    {
        "solution-architecture.puml",
        "domain-models.puml",
        "cqrs-pattern.puml",
        "api-endpoints.puml",
        "database-architecture.puml",
        "authentication-flow.puml",
        "angular-architecture.puml",
        "angular-routing.puml",
        "angular-http.puml"
    };

    /// <summary>
    /// Recommended (but not required) document files.
    /// </summary>
    private static readonly HashSet<string> RecommendedDocuments = new(StringComparer.OrdinalIgnoreCase)
    {
        "solution-architecture.puml",
        "domain-models.puml"
    };

    /// <summary>
    /// Valid primitive types recognized by the scaffolder.
    /// </summary>
    private static readonly HashSet<string> ValidPrimitiveTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "string", "int", "long", "decimal", "double", "float", "bool", "boolean",
        "DateTime", "DateTimeOffset", "Guid", "byte", "short", "char",
        "void", "object", "dynamic"
    };

    /// <summary>
    /// Valid collection type wrappers.
    /// </summary>
    private static readonly HashSet<string> ValidCollectionTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "List", "IList", "ICollection", "IEnumerable", "Collection", "HashSet", "ISet",
        "Dictionary", "IDictionary", "Array"
    };

    /// <summary>
    /// Valid stereotypes as defined in the specification.
    /// </summary>
    private static readonly HashSet<string> ValidStereotypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "aggregate", "entity", "valueobject", "enum", "service"
    };

    /// <summary>
    /// Audit field names that should be automatically generated.
    /// </summary>
    private static readonly HashSet<string> AuditFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "CreatedAt", "ModifiedAt", "CreatedBy", "ModifiedBy"
    };

    public PlantUmlValidationService(
        ILogger<PlantUmlValidationService> logger,
        IPlantUmlParserService parserService,
        IFileSystem fileSystem)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.parserService = parserService ?? throw new ArgumentNullException(nameof(parserService));
        this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public async Task<PlantUmlValidationResult> ValidateDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Validating PlantUML files in directory: {Directory}", directoryPath);

        var result = new PlantUmlValidationResult
        {
            SourcePath = directoryPath,
            ValidatedAt = DateTime.UtcNow
        };

        // Check if directory exists
        if (!fileSystem.Directory.Exists(directoryPath))
        {
            result.GlobalIssues.Add(new PlantUmlValidationIssue
            {
                Code = "DIR001",
                Severity = ValidationSeverity.Error,
                Category = ValidationCategory.DocumentStructure,
                Message = "PlantUML source directory does not exist",
                Details = $"Directory not found: {directoryPath}",
                SuggestedFix = "Ensure the directory path is correct and the directory exists"
            });
            return result;
        }

        // Parse all PlantUML files
        var model = await parserService.ParseDirectoryAsync(directoryPath, cancellationToken);

        // Validate the parsed model
        var modelValidation = ValidateModel(model);

        // Merge results
        result.DocumentResults.AddRange(modelValidation.DocumentResults);
        result.GlobalIssues.AddRange(modelValidation.GlobalIssues);

        // Additional file-level validations (raw content checks)
        await ValidateRawFileContentsAsync(directoryPath, result, cancellationToken);

        logger.LogInformation("Validation complete. Errors: {Errors}, Warnings: {Warnings}",
            result.TotalErrors, result.TotalWarnings);

        return result;
    }

    public async Task<PlantUmlDocumentValidationResult> ValidateFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Validating PlantUML file: {File}", filePath);

        var result = new PlantUmlDocumentValidationResult
        {
            FilePath = filePath
        };

        if (!fileSystem.File.Exists(filePath))
        {
            result.Issues.Add(new PlantUmlValidationIssue
            {
                Code = "FILE001",
                Severity = ValidationSeverity.Error,
                Category = ValidationCategory.DocumentStructure,
                Message = "PlantUML file does not exist",
                Details = $"File not found: {filePath}"
            });
            return result;
        }

        var content = await Task.Run(() => fileSystem.File.ReadAllText(filePath), cancellationToken);
        ValidateRawContent(content, filePath, result);

        var document = await parserService.ParseFileAsync(filePath, cancellationToken);
        if (document != null)
        {
            result.DocumentTitle = document.Title;
            ValidateDocument(document, result);
        }

        return result;
    }

    public PlantUmlValidationResult ValidateModel(PlantUmlSolutionModel model)
    {
        var result = new PlantUmlValidationResult
        {
            SourcePath = model.SourcePath,
            ValidatedAt = DateTime.UtcNow
        };

        // Validate required documents
        ValidateRequiredDocuments(model, result);

        // Validate each document
        foreach (var document in model.Documents)
        {
            var docResult = new PlantUmlDocumentValidationResult
            {
                FilePath = document.FilePath,
                DocumentTitle = document.Title
            };

            ValidateDocument(document, docResult);
            result.DocumentResults.Add(docResult);
        }

        // Cross-document validations
        ValidateCrossDocumentConsistency(model, result);

        return result;
    }

    private void ValidateRequiredDocuments(PlantUmlSolutionModel model, PlantUmlValidationResult result)
    {
        var existingFiles = model.Documents
            .Where(d => !string.IsNullOrEmpty(d.FilePath))
            .Select(d => Path.GetFileName(d.FilePath))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Check for domain-models.puml - this is critical
        if (!existingFiles.Any(f => f.Contains("domain", StringComparison.OrdinalIgnoreCase) &&
                                   f.EndsWith(".puml", StringComparison.OrdinalIgnoreCase)))
        {
            // Check if there are any entities defined
            if (!model.GetAllClasses().Any())
            {
                result.GlobalIssues.Add(new PlantUmlValidationIssue
                {
                    Code = "REQ001",
                    Severity = ValidationSeverity.Error,
                    Category = ValidationCategory.MissingDocument,
                    Message = "No domain models found",
                    Details = "No classes or entities were found in any PlantUML files. At least one aggregate or entity must be defined.",
                    SuggestedFix = "Create a domain-models.puml file with at least one class definition using the <<aggregate>> or <<entity>> stereotype"
                });
            }
        }

        // Check for recommended documents (warnings, not errors)
        foreach (var recommended in RecommendedDocuments)
        {
            if (!existingFiles.Contains(recommended))
            {
                result.GlobalIssues.Add(new PlantUmlValidationIssue
                {
                    Code = "REC001",
                    Severity = ValidationSeverity.Info,
                    Category = ValidationCategory.MissingDocument,
                    Message = $"Recommended document not found: {recommended}",
                    Details = $"The file '{recommended}' is recommended for a complete solution architecture but is optional.",
                    SuggestedFix = $"Consider creating '{recommended}' for better documentation"
                });
            }
        }

        // Check if at least one document was found
        if (model.Documents.Count == 0)
        {
            result.GlobalIssues.Add(new PlantUmlValidationIssue
            {
                Code = "REQ002",
                Severity = ValidationSeverity.Error,
                Category = ValidationCategory.MissingDocument,
                Message = "No PlantUML documents found",
                Details = "The specified directory contains no .puml files",
                SuggestedFix = "Add at least one .puml file with entity definitions to the directory"
            });
        }
    }

    private void ValidateDocument(PlantUmlDocumentModel document, PlantUmlDocumentValidationResult result)
    {
        // Validate all classes
        foreach (var package in document.Packages)
        {
            ValidatePackage(package, result);
        }

        foreach (var cls in document.Classes)
        {
            var entityResult = ValidateClass(cls, document);
            if (entityResult.Issues.Count > 0)
            {
                result.EntityResults.Add(entityResult);
            }
        }

        foreach (var enm in document.Enums)
        {
            var entityResult = ValidateEnum(enm);
            if (entityResult.Issues.Count > 0)
            {
                result.EntityResults.Add(entityResult);
            }
        }

        // Validate relationships
        ValidateRelationships(document, result);

        // Validate components
        ValidateComponents(document, result);
    }

    private void ValidatePackage(PlantUmlPackageModel package, PlantUmlDocumentValidationResult result)
    {
        // Validate package naming convention
        if (!string.IsNullOrEmpty(package.Name))
        {
            var parts = package.Name.Split('.');

            // Should follow: {SolutionName}.{BoundedContext}.Aggregates.{EntityName} or {SolutionName}.Aggregates.{EntityName}
            if (!parts.Any(p => p.Equals("Aggregates", StringComparison.OrdinalIgnoreCase)))
            {
                result.Issues.Add(new PlantUmlValidationIssue
                {
                    Code = "PKG001",
                    Severity = ValidationSeverity.Warning,
                    Category = ValidationCategory.Naming,
                    Message = "Package name does not follow expected convention",
                    Details = $"Package '{package.Name}' should include 'Aggregates' segment (e.g., 'SolutionName.Aggregates.EntityName' or 'SolutionName.BoundedContext.Aggregates.EntityName')",
                    SuggestedFix = "Rename the package to follow the pattern: {SolutionName}[.{BoundedContext}].Aggregates.{EntityName}",
                    RelatedElement = package.Name
                });
            }

            // Validate classes within package
            foreach (var cls in package.Classes)
            {
                var entityResult = ValidateClass(cls, null);
                if (entityResult.Issues.Count > 0)
                {
                    result.EntityResults.Add(entityResult);
                }
            }

            // Validate enums within package
            foreach (var enm in package.Enums)
            {
                var entityResult = ValidateEnum(enm);
                if (entityResult.Issues.Count > 0)
                {
                    result.EntityResults.Add(entityResult);
                }
            }
        }
    }

    private PlantUmlEntityValidationResult ValidateClass(PlantUmlClassModel cls, PlantUmlDocumentModel document)
    {
        var result = new PlantUmlEntityValidationResult
        {
            EntityName = cls.Name,
            EntityType = cls.Stereotype == PlantUmlStereotype.None ? "Class" : cls.Stereotype.ToString(),
            Namespace = cls.Namespace
        };

        // Validate class name
        if (string.IsNullOrWhiteSpace(cls.Name))
        {
            result.Issues.Add(new PlantUmlValidationIssue
            {
                Code = "CLS001",
                Severity = ValidationSeverity.Error,
                Category = ValidationCategory.EntityDefinition,
                Message = "Class has no name",
                SuggestedFix = "Provide a valid class name"
            });
            return result;
        }

        // Validate class name is PascalCase
        if (!char.IsUpper(cls.Name[0]))
        {
            result.Issues.Add(new PlantUmlValidationIssue
            {
                Code = "CLS002",
                Severity = ValidationSeverity.Warning,
                Category = ValidationCategory.Naming,
                Message = "Class name should start with uppercase letter (PascalCase)",
                Details = $"Class name '{cls.Name}' starts with lowercase",
                SuggestedFix = $"Rename to '{char.ToUpper(cls.Name[0])}{cls.Name[1..]}'",
                RelatedElement = cls.Name
            });
        }

        // For aggregates and entities, validate primary key
        if (cls.IsAggregate || cls.IsEntity)
        {
            ValidateEntityPrimaryKey(cls, result);
            ValidateEntityProperties(cls, result);
        }

        // Validate stereotype if present
        if (cls.Stereotype != PlantUmlStereotype.None)
        {
            // Stereotype is already validated by the parser, but we can add additional checks
        }

        // Validate all properties
        foreach (var prop in cls.Properties)
        {
            ValidateProperty(prop, cls.Name, result);
        }

        // Check for empty class
        if (cls.Properties.Count == 0 && cls.Methods.Count == 0)
        {
            result.Issues.Add(new PlantUmlValidationIssue
            {
                Code = "CLS003",
                Severity = ValidationSeverity.Warning,
                Category = ValidationCategory.EntityDefinition,
                Message = "Class has no properties or methods",
                Details = $"Class '{cls.Name}' is empty",
                SuggestedFix = "Add at least one property to the class",
                RelatedElement = cls.Name
            });
        }

        return result;
    }

    private void ValidateEntityPrimaryKey(PlantUmlClassModel cls, PlantUmlEntityValidationResult result)
    {
        var expectedKeyName = $"{cls.Name}Id";
        var keyProperty = cls.Properties.FirstOrDefault(p => p.IsKey);

        if (keyProperty == null)
        {
            // Check if there's a property that looks like a key but wasn't detected
            var possibleKey = cls.Properties.FirstOrDefault(p =>
                p.Name.Equals(expectedKeyName, StringComparison.OrdinalIgnoreCase));

            if (possibleKey == null)
            {
                result.Issues.Add(new PlantUmlValidationIssue
                {
                    Code = "KEY001",
                    Severity = ValidationSeverity.Error,
                    Category = ValidationCategory.EntityDefinition,
                    Message = $"Entity is missing required primary key property",
                    Details = $"Entity '{cls.Name}' must have a property named '{expectedKeyName}' of type 'string'",
                    SuggestedFix = $"Add property: +{expectedKeyName} : string",
                    RelatedElement = cls.Name
                });
            }
            else
            {
                result.Issues.Add(new PlantUmlValidationIssue
                {
                    Code = "KEY002",
                    Severity = ValidationSeverity.Warning,
                    Category = ValidationCategory.EntityDefinition,
                    Message = "Primary key property found but not marked as key",
                    Details = $"Property '{possibleKey.Name}' appears to be the primary key but was not recognized",
                    SuggestedFix = "Ensure the property follows the format: +{ClassName}Id : string",
                    RelatedElement = possibleKey.Name
                });
            }
        }
        else if (!keyProperty.Name.Equals(expectedKeyName, StringComparison.OrdinalIgnoreCase))
        {
            result.Issues.Add(new PlantUmlValidationIssue
            {
                Code = "KEY003",
                Severity = ValidationSeverity.Warning,
                Category = ValidationCategory.Naming,
                Message = "Primary key property does not follow naming convention",
                Details = $"Expected '{expectedKeyName}' but found '{keyProperty.Name}'",
                SuggestedFix = $"Rename the property to '{expectedKeyName}'",
                RelatedElement = keyProperty.Name
            });
        }

        // Validate key type is string
        if (keyProperty != null && !keyProperty.Type.Equals("string", StringComparison.OrdinalIgnoreCase) &&
            !keyProperty.Type.Equals("Guid", StringComparison.OrdinalIgnoreCase))
        {
            result.Issues.Add(new PlantUmlValidationIssue
            {
                Code = "KEY004",
                Severity = ValidationSeverity.Warning,
                Category = ValidationCategory.PropertyDefinition,
                Message = "Primary key should be of type 'string' or 'Guid'",
                Details = $"Property '{keyProperty.Name}' is of type '{keyProperty.Type}'",
                SuggestedFix = "Change the property type to 'string'",
                RelatedElement = keyProperty.Name
            });
        }
    }

    private void ValidateEntityProperties(PlantUmlClassModel cls, PlantUmlEntityValidationResult result)
    {
        // Check for audit fields (info, not required)
        var hasCreatedAt = cls.Properties.Any(p => p.Name.Equals("CreatedAt", StringComparison.OrdinalIgnoreCase));
        var hasModifiedAt = cls.Properties.Any(p => p.Name.Equals("ModifiedAt", StringComparison.OrdinalIgnoreCase));

        if (!hasCreatedAt || !hasModifiedAt)
        {
            result.Issues.Add(new PlantUmlValidationIssue
            {
                Code = "AUD001",
                Severity = ValidationSeverity.Info,
                Category = ValidationCategory.BestPractice,
                Message = "Entity is missing audit fields",
                Details = "Audit fields (CreatedAt, ModifiedAt) are recommended for tracking entity changes. They will be automatically added during scaffolding.",
                SuggestedFix = "Consider adding: +CreatedAt : DateTime and +ModifiedAt : DateTime",
                RelatedElement = cls.Name
            });
        }
    }

    private void ValidateProperty(PlantUmlPropertyModel prop, string className, PlantUmlEntityValidationResult result)
    {
        // Validate property name
        if (string.IsNullOrWhiteSpace(prop.Name))
        {
            result.Issues.Add(new PlantUmlValidationIssue
            {
                Code = "PROP001",
                Severity = ValidationSeverity.Error,
                Category = ValidationCategory.PropertyDefinition,
                Message = "Property has no name",
                SuggestedFix = "Provide a valid property name",
                RelatedElement = className
            });
            return;
        }

        // Validate property name is PascalCase
        if (!char.IsUpper(prop.Name[0]))
        {
            result.Issues.Add(new PlantUmlValidationIssue
            {
                Code = "PROP002",
                Severity = ValidationSeverity.Warning,
                Category = ValidationCategory.Naming,
                Message = "Property name should start with uppercase letter (PascalCase)",
                Details = $"Property '{prop.Name}' starts with lowercase",
                SuggestedFix = $"Rename to '{char.ToUpper(prop.Name[0])}{prop.Name[1..]}'",
                RelatedElement = prop.Name
            });
        }

        // Validate type is specified
        if (string.IsNullOrWhiteSpace(prop.Type) && string.IsNullOrWhiteSpace(prop.CollectionType))
        {
            result.Issues.Add(new PlantUmlValidationIssue
            {
                Code = "PROP003",
                Severity = ValidationSeverity.Error,
                Category = ValidationCategory.PropertyDefinition,
                Message = "Property has no type specified",
                Details = $"Property '{prop.Name}' is missing type annotation",
                SuggestedFix = $"Add type annotation: +{prop.Name} : TypeName",
                RelatedElement = prop.Name
            });
        }

        // Validate collection types
        if (prop.IsCollection && !string.IsNullOrEmpty(prop.CollectionType))
        {
            if (!ValidCollectionTypes.Contains(prop.CollectionType))
            {
                result.Issues.Add(new PlantUmlValidationIssue
                {
                    Code = "PROP004",
                    Severity = ValidationSeverity.Warning,
                    Category = ValidationCategory.PropertyDefinition,
                    Message = $"Unrecognized collection type: {prop.CollectionType}",
                    Details = $"Property '{prop.Name}' uses collection type '{prop.CollectionType}'",
                    SuggestedFix = "Use a standard collection type: List<T>, ICollection<T>, IEnumerable<T>, etc.",
                    RelatedElement = prop.Name
                });
            }
        }
    }

    private PlantUmlEntityValidationResult ValidateEnum(PlantUmlEnumModel enm)
    {
        var result = new PlantUmlEntityValidationResult
        {
            EntityName = enm.Name,
            EntityType = "Enum",
            Namespace = enm.Namespace
        };

        // Validate enum name
        if (string.IsNullOrWhiteSpace(enm.Name))
        {
            result.Issues.Add(new PlantUmlValidationIssue
            {
                Code = "ENUM001",
                Severity = ValidationSeverity.Error,
                Category = ValidationCategory.EntityDefinition,
                Message = "Enum has no name",
                SuggestedFix = "Provide a valid enum name"
            });
            return result;
        }

        // Validate enum name is PascalCase
        if (!char.IsUpper(enm.Name[0]))
        {
            result.Issues.Add(new PlantUmlValidationIssue
            {
                Code = "ENUM002",
                Severity = ValidationSeverity.Warning,
                Category = ValidationCategory.Naming,
                Message = "Enum name should start with uppercase letter (PascalCase)",
                Details = $"Enum name '{enm.Name}' starts with lowercase",
                SuggestedFix = $"Rename to '{char.ToUpper(enm.Name[0])}{enm.Name[1..]}'",
                RelatedElement = enm.Name
            });
        }

        // Validate enum has values
        if (enm.Values == null || enm.Values.Count == 0)
        {
            result.Issues.Add(new PlantUmlValidationIssue
            {
                Code = "ENUM003",
                Severity = ValidationSeverity.Error,
                Category = ValidationCategory.EntityDefinition,
                Message = "Enum has no values defined",
                Details = $"Enum '{enm.Name}' is empty",
                SuggestedFix = "Add at least one value to the enum",
                RelatedElement = enm.Name
            });
        }
        else
        {
            // Validate enum values are PascalCase
            foreach (var value in enm.Values)
            {
                if (!string.IsNullOrEmpty(value) && !char.IsUpper(value[0]))
                {
                    result.Issues.Add(new PlantUmlValidationIssue
                    {
                        Code = "ENUM004",
                        Severity = ValidationSeverity.Warning,
                        Category = ValidationCategory.Naming,
                        Message = "Enum value should start with uppercase letter (PascalCase)",
                        Details = $"Enum value '{value}' in '{enm.Name}' starts with lowercase",
                        SuggestedFix = $"Rename to '{char.ToUpper(value[0])}{value[1..]}'",
                        RelatedElement = value
                    });
                }
            }
        }

        return result;
    }

    private void ValidateRelationships(PlantUmlDocumentModel document, PlantUmlDocumentValidationResult result)
    {
        // Get all known class and enum names in this document
        var knownTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var cls in document.Classes)
        {
            knownTypes.Add(cls.Name);
        }

        foreach (var pkg in document.Packages)
        {
            foreach (var cls in pkg.Classes)
            {
                knownTypes.Add(cls.Name);
            }

            foreach (var enm in pkg.Enums)
            {
                knownTypes.Add(enm.Name);
            }
        }

        foreach (var enm in document.Enums)
        {
            knownTypes.Add(enm.Name);
        }

        // Validate each relationship
        foreach (var rel in document.Relationships)
        {
            // Validate source class exists
            if (!knownTypes.Contains(rel.SourceClass) && !ValidPrimitiveTypes.Contains(rel.SourceClass))
            {
                result.Issues.Add(new PlantUmlValidationIssue
                {
                    Code = "REL001",
                    Severity = ValidationSeverity.Warning,
                    Category = ValidationCategory.Relationship,
                    Message = $"Relationship references unknown source class: {rel.SourceClass}",
                    Details = $"Class '{rel.SourceClass}' is not defined in this document",
                    SuggestedFix = "Define the class or fix the relationship reference",
                    RelatedElement = rel.SourceClass
                });
            }

            // Validate target class exists
            if (!knownTypes.Contains(rel.TargetClass) && !ValidPrimitiveTypes.Contains(rel.TargetClass))
            {
                result.Issues.Add(new PlantUmlValidationIssue
                {
                    Code = "REL002",
                    Severity = ValidationSeverity.Warning,
                    Category = ValidationCategory.Relationship,
                    Message = $"Relationship references unknown target class: {rel.TargetClass}",
                    Details = $"Class '{rel.TargetClass}' is not defined in this document",
                    SuggestedFix = "Define the class or fix the relationship reference",
                    RelatedElement = rel.TargetClass
                });
            }

            // Validate cardinality for composition/aggregation
            if ((rel.RelationshipType == PlantUmlRelationshipType.Composition ||
                 rel.RelationshipType == PlantUmlRelationshipType.Aggregation) &&
                string.IsNullOrEmpty(rel.SourceCardinality) && string.IsNullOrEmpty(rel.TargetCardinality))
            {
                result.Issues.Add(new PlantUmlValidationIssue
                {
                    Code = "REL003",
                    Severity = ValidationSeverity.Info,
                    Category = ValidationCategory.Relationship,
                    Message = "Relationship is missing cardinality",
                    Details = $"Relationship between '{rel.SourceClass}' and '{rel.TargetClass}' has no cardinality specified",
                    SuggestedFix = "Add cardinality (e.g., \"1\" *-- \"0..*\" for one-to-many)",
                    RelatedElement = $"{rel.SourceClass} -> {rel.TargetClass}"
                });
            }
        }
    }

    private void ValidateComponents(PlantUmlDocumentModel document, PlantUmlDocumentValidationResult result)
    {
        var componentNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var componentAliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var component in document.Components)
        {
            // Check for duplicate component names
            if (!componentNames.Add(component.Name))
            {
                result.Issues.Add(new PlantUmlValidationIssue
                {
                    Code = "COMP001",
                    Severity = ValidationSeverity.Error,
                    Category = ValidationCategory.Duplicate,
                    Message = $"Duplicate component name: {component.Name}",
                    SuggestedFix = "Use unique component names",
                    RelatedElement = component.Name
                });
            }

            // Check for duplicate aliases
            if (!string.IsNullOrEmpty(component.Alias) && !componentAliases.Add(component.Alias))
            {
                result.Issues.Add(new PlantUmlValidationIssue
                {
                    Code = "COMP002",
                    Severity = ValidationSeverity.Error,
                    Category = ValidationCategory.Duplicate,
                    Message = $"Duplicate component alias: {component.Alias}",
                    SuggestedFix = "Use unique component aliases",
                    RelatedElement = component.Alias
                });
            }

            // Validate controller components have endpoint specifications
            if (component.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase) &&
                component.CodeGeneratorSpec == null)
            {
                result.Issues.Add(new PlantUmlValidationIssue
                {
                    Code = "COMP003",
                    Severity = ValidationSeverity.Info,
                    Category = ValidationCategory.BestPractice,
                    Message = $"Controller component is missing endpoint specification",
                    Details = $"Component '{component.Name}' appears to be a controller but has no endpoint note",
                    SuggestedFix = "Add a note with endpoint specifications (Route, HTTP verbs, etc.)",
                    RelatedElement = component.Name
                });
            }
        }
    }

    private void ValidateCrossDocumentConsistency(PlantUmlSolutionModel model, PlantUmlValidationResult result)
    {
        var allClasses = model.GetAllClasses().ToList();
        var allEnums = model.GetAllEnums().ToList();
        var allTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Build set of all defined types
        foreach (var cls in allClasses)
        {
            allTypes.Add(cls.Name);
        }

        foreach (var enm in allEnums)
        {
            allTypes.Add(enm.Name);
        }

        // Check for duplicate class definitions across documents
        var classNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var doc in model.Documents)
        {
            foreach (var cls in doc.Classes.Concat(doc.Packages.SelectMany(p => p.Classes)))
            {
                if (classNames.TryGetValue(cls.Name, out var existingFile))
                {
                    if (!string.Equals(existingFile, doc.FilePath, StringComparison.OrdinalIgnoreCase))
                    {
                        result.GlobalIssues.Add(new PlantUmlValidationIssue
                        {
                            Code = "DUP001",
                            Severity = ValidationSeverity.Error,
                            Category = ValidationCategory.Duplicate,
                            Message = $"Duplicate class definition: {cls.Name}",
                            Details = $"Class '{cls.Name}' is defined in both '{Path.GetFileName(existingFile)}' and '{Path.GetFileName(doc.FilePath)}'",
                            SuggestedFix = "Remove the duplicate definition or rename one of the classes",
                            RelatedElement = cls.Name
                        });
                    }
                }
                else
                {
                    classNames[cls.Name] = doc.FilePath;
                }
            }
        }

        // Check for duplicate enum definitions
        var enumNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var doc in model.Documents)
        {
            foreach (var enm in doc.Enums.Concat(doc.Packages.SelectMany(p => p.Enums)))
            {
                if (enumNames.TryGetValue(enm.Name, out var existingFile))
                {
                    if (!string.Equals(existingFile, doc.FilePath, StringComparison.OrdinalIgnoreCase))
                    {
                        result.GlobalIssues.Add(new PlantUmlValidationIssue
                        {
                            Code = "DUP002",
                            Severity = ValidationSeverity.Error,
                            Category = ValidationCategory.Duplicate,
                            Message = $"Duplicate enum definition: {enm.Name}",
                            Details = $"Enum '{enm.Name}' is defined in both '{Path.GetFileName(existingFile)}' and '{Path.GetFileName(doc.FilePath)}'",
                            SuggestedFix = "Remove the duplicate definition or rename one of the enums",
                            RelatedElement = enm.Name
                        });
                    }
                }
                else
                {
                    enumNames[enm.Name] = doc.FilePath;
                }
            }
        }

        // Validate type references across all classes
        foreach (var cls in allClasses)
        {
            foreach (var prop in cls.Properties)
            {
                var typeToCheck = prop.IsCollection ? prop.GenericTypeArgument : prop.Type;

                if (!string.IsNullOrEmpty(typeToCheck) &&
                    !ValidPrimitiveTypes.Contains(typeToCheck) &&
                    !allTypes.Contains(typeToCheck))
                {
                    result.GlobalIssues.Add(new PlantUmlValidationIssue
                    {
                        Code = "TYPE001",
                        Severity = ValidationSeverity.Warning,
                        Category = ValidationCategory.TypeReference,
                        Message = $"Property references undefined type: {typeToCheck}",
                        Details = $"Property '{prop.Name}' in class '{cls.Name}' references type '{typeToCheck}' which is not defined",
                        SuggestedFix = $"Define the type '{typeToCheck}' or use a valid type",
                        RelatedElement = $"{cls.Name}.{prop.Name}"
                    });
                }
            }
        }

        // Validate bounded contexts consistency
        var boundedContexts = model.GetBoundedContexts().ToList();
        if (boundedContexts.Count > 0)
        {
            result.GlobalIssues.Add(new PlantUmlValidationIssue
            {
                Code = "BC001",
                Severity = ValidationSeverity.Info,
                Category = ValidationCategory.Consistency,
                Message = $"Found {boundedContexts.Count} bounded context(s)",
                Details = $"Bounded contexts: {string.Join(", ", boundedContexts)}",
                SuggestedFix = "Ensure all entities are correctly assigned to their bounded contexts"
            });
        }

        // Check for aggregates without relationships
        var aggregates = model.GetAggregates().ToList();
        var allRelationships = model.GetAllRelationships().ToList();

        foreach (var agg in aggregates)
        {
            var hasChildren = allRelationships.Any(r =>
                r.SourceClass.Equals(agg.Name, StringComparison.OrdinalIgnoreCase) &&
                (r.RelationshipType == PlantUmlRelationshipType.Composition ||
                 r.RelationshipType == PlantUmlRelationshipType.Aggregation));

            var hasCollectionProperties = agg.Properties.Any(p => p.IsCollection);

            if (!hasChildren && hasCollectionProperties)
            {
                result.GlobalIssues.Add(new PlantUmlValidationIssue
                {
                    Code = "AGG001",
                    Severity = ValidationSeverity.Info,
                    Category = ValidationCategory.Relationship,
                    Message = $"Aggregate has collection properties but no explicit relationships",
                    Details = $"Aggregate '{agg.Name}' has collection properties. Consider adding explicit relationships for clarity.",
                    SuggestedFix = "Add relationships like: Parent \"1\" *-- \"0..*\" Child : contains",
                    RelatedElement = agg.Name
                });
            }
        }

        // Check if there are any aggregates defined
        if (aggregates.Count == 0 && allClasses.Count > 0)
        {
            result.GlobalIssues.Add(new PlantUmlValidationIssue
            {
                Code = "AGG002",
                Severity = ValidationSeverity.Warning,
                Category = ValidationCategory.Stereotype,
                Message = "No aggregate roots defined",
                Details = $"Found {allClasses.Count} class(es) but none are marked with <<aggregate>> stereotype",
                SuggestedFix = "Mark at least one entity as an aggregate root using the <<aggregate>> stereotype"
            });
        }
    }

    private async Task ValidateRawFileContentsAsync(string directoryPath, PlantUmlValidationResult result, CancellationToken cancellationToken)
    {
        var pumlFiles = fileSystem.Directory.GetFiles(directoryPath, "*.puml", SearchOption.AllDirectories);

        foreach (var file in pumlFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var content = await Task.Run(() => fileSystem.File.ReadAllText(file), cancellationToken);
                var docResult = result.DocumentResults.FirstOrDefault(d =>
                    string.Equals(d.FilePath, file, StringComparison.OrdinalIgnoreCase));

                if (docResult == null)
                {
                    docResult = new PlantUmlDocumentValidationResult { FilePath = file };
                    result.DocumentResults.Add(docResult);
                }

                ValidateRawContent(content, file, docResult);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error reading file for raw validation: {File}", file);
            }
        }
    }

    private void ValidateRawContent(string content, string filePath, PlantUmlDocumentValidationResult result)
    {
        var lines = content.Split('\n');

        // Check for @startuml and @enduml
        var hasStartUml = content.Contains("@startuml", StringComparison.OrdinalIgnoreCase);
        var hasEndUml = content.Contains("@enduml", StringComparison.OrdinalIgnoreCase);

        if (!hasStartUml)
        {
            result.Issues.Add(new PlantUmlValidationIssue
            {
                Code = "STRUCT001",
                Severity = ValidationSeverity.Error,
                Category = ValidationCategory.DocumentStructure,
                Message = "Missing @startuml declaration",
                Details = "Every PlantUML document must begin with @startuml",
                SuggestedFix = "Add @startuml at the beginning of the file"
            });
        }

        if (!hasEndUml)
        {
            result.Issues.Add(new PlantUmlValidationIssue
            {
                Code = "STRUCT002",
                Severity = ValidationSeverity.Error,
                Category = ValidationCategory.DocumentStructure,
                Message = "Missing @enduml declaration",
                Details = "Every PlantUML document must end with @enduml",
                SuggestedFix = "Add @enduml at the end of the file"
            });
        }

        // Check for unclosed blocks
        ValidateBlockClosure(content, "class", "{", "}", result);
        ValidateBlockClosure(content, "enum", "{", "}", result);
        ValidateBlockClosure(content, "package", "{", "}", result);

        // Check for unclosed note blocks
        var noteStartCount = Regex.Matches(content, @"\bnote\b", RegexOptions.IgnoreCase).Count;
        var noteEndCount = Regex.Matches(content, @"\bend note\b", RegexOptions.IgnoreCase).Count;

        if (noteStartCount > noteEndCount)
        {
            result.Issues.Add(new PlantUmlValidationIssue
            {
                Code = "STRUCT003",
                Severity = ValidationSeverity.Error,
                Category = ValidationCategory.DocumentStructure,
                Message = "Unclosed note block detected",
                Details = $"Found {noteStartCount} note declarations but only {noteEndCount} 'end note' closures",
                SuggestedFix = "Ensure all note blocks are closed with 'end note'"
            });
        }

        // Check for multi-line component blocks (invalid in PlantUML)
        var componentBlockPattern = @"component\s+""[^""]*""\s+as\s+\w+\s*\{[^}]+\}";
        if (Regex.IsMatch(content, componentBlockPattern, RegexOptions.Singleline))
        {
            result.Issues.Add(new PlantUmlValidationIssue
            {
                Code = "STRUCT004",
                Severity = ValidationSeverity.Error,
                Category = ValidationCategory.DocumentStructure,
                Message = "Invalid multi-line component block detected",
                Details = "Components cannot have multi-line content inside braces. Use notes instead.",
                SuggestedFix = "Use 'component [Name] as alias' with a separate 'note right of alias' block"
            });
        }

        // Check for valid title
        var titleMatch = Regex.Match(content, @"title\s+(.+)", RegexOptions.Multiline);
        if (!titleMatch.Success && hasStartUml)
        {
            result.Issues.Add(new PlantUmlValidationIssue
            {
                Code = "STRUCT005",
                Severity = ValidationSeverity.Info,
                Category = ValidationCategory.BestPractice,
                Message = "Document is missing a title",
                SuggestedFix = "Add a title: title Document Title - Version X.X"
            });
        }
    }

    private void ValidateBlockClosure(string content, string blockType, string openBrace, string closeBrace, PlantUmlDocumentValidationResult result)
    {
        var pattern = $@"\b{blockType}\b";
        var blockStarts = Regex.Matches(content, pattern, RegexOptions.IgnoreCase).Count;

        if (blockStarts > 0)
        {
            // Simple brace counting for the whole document
            var openCount = content.Count(c => c == openBrace[0]);
            var closeCount = content.Count(c => c == closeBrace[0]);

            if (openCount != closeCount)
            {
                result.Issues.Add(new PlantUmlValidationIssue
                {
                    Code = "STRUCT006",
                    Severity = ValidationSeverity.Error,
                    Category = ValidationCategory.DocumentStructure,
                    Message = $"Mismatched braces detected",
                    Details = $"Found {openCount} opening braces and {closeCount} closing braces",
                    SuggestedFix = "Ensure all blocks are properly closed with matching braces"
                });
            }
        }
    }
}
