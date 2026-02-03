// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeGenerator.DotNet.Artifacts.PlantUml.Models;
using Microsoft.Extensions.Logging;

namespace CodeGenerator.DotNet.Artifacts.PlantUml.Services;

public class SequenceToSolutionPlantUmlService : ISequenceToSolutionPlantUmlService
{
    private readonly ILogger<SequenceToSolutionPlantUmlService> _logger;
    private readonly IPlantUmlParserService _parserService;

    public SequenceToSolutionPlantUmlService(
        ILogger<SequenceToSolutionPlantUmlService> logger,
        IPlantUmlParserService parserService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _parserService = parserService ?? throw new ArgumentNullException(nameof(parserService));
    }

    public string GenerateSolutionPlantUml(string sequenceDiagramContent, string solutionName)
    {
        _logger.LogInformation("Generating solution PlantUML from sequence diagram for solution: {SolutionName}", solutionName);

        // Parse the sequence diagram to extract participants
        var document = _parserService.ParseContent(sequenceDiagramContent);

        if (!document.Participants.Any())
        {
            _logger.LogWarning("No participants found in sequence diagram");
            return GenerateDefaultSolutionPlantUml(solutionName);
        }

        var sb = new StringBuilder();

        // Group participants by their DotNetType
        var participantsByType = document.Participants
            .GroupBy(p => p.DotNetType ?? "Unknown")
            .ToDictionary(g => g.Key, g => g.ToList());

        _logger.LogInformation("Found participants: {Count} groups", participantsByType.Count);

        // Generate domain models document
        sb.AppendLine(GenerateDomainModelsDocument(participantsByType, solutionName));
        sb.AppendLine();

        // Generate solution architecture document
        sb.AppendLine(GenerateSolutionArchitectureDocument(participantsByType, solutionName));

        return sb.ToString();
    }

    private string GenerateDomainModelsDocument(Dictionary<string, List<PlantUmlParticipantModel>> participantsByType, string solutionName)
    {
        var sb = new StringBuilder();

        sb.AppendLine("@startuml Domain Models");
        sb.AppendLine("!pragma layout smetana");
        sb.AppendLine("skinparam backgroundColor #FEFEFE");
        sb.AppendLine("skinparam defaultFontSize 11");
        sb.AppendLine();
        sb.AppendLine($"title {solutionName} - Domain Models");
        sb.AppendLine();

        // Generate entities for Microservices
        if (participantsByType.TryGetValue("Microservice", out var microservices))
        {
            foreach (var microservice in microservices)
            {
                var entityName = SanitizeName(microservice.Name);
                var boundedContext = entityName;

                sb.AppendLine($"package \"{solutionName}.{boundedContext}.Aggregates.{entityName}\" {{");
                sb.AppendLine($"    class {entityName} <<aggregate>> {{");
                sb.AppendLine($"        +{entityName}Id : string");
                sb.AppendLine($"        +Name : string");
                sb.AppendLine($"        +Description : string");
                sb.AppendLine($"        +Status : string");
                sb.AppendLine($"        +CreatedAt : DateTime");
                sb.AppendLine($"        +ModifiedAt : DateTime");
                sb.AppendLine("    }");
                sb.AppendLine("}");
                sb.AppendLine();
            }
        }

        // Workers don't typically need domain models, they process messages

        sb.AppendLine("@enduml");

        return sb.ToString();
    }

    private string GenerateSolutionArchitectureDocument(Dictionary<string, List<PlantUmlParticipantModel>> participantsByType, string solutionName)
    {
        var sb = new StringBuilder();

        sb.AppendLine("@startuml Solution Architecture");
        sb.AppendLine("!pragma layout smetana");
        sb.AppendLine("skinparam backgroundColor #FEFEFE");
        sb.AppendLine("skinparam defaultFontSize 11");
        sb.AppendLine();
        sb.AppendLine($"title {solutionName} - Solution Architecture");
        sb.AppendLine();

        // Generate Angular projects
        if (participantsByType.TryGetValue("Angular", out var angularProjects) || participantsByType.TryGetValue("ts", out angularProjects))
        {
            sb.AppendLine("package \"Frontend\" {");
            foreach (var angular in angularProjects)
            {
                var projectName = SanitizeName(angular.Name);
                sb.AppendLine($"    component [{projectName}] as {angular.Alias} <<Angular>>");
            }
            sb.AppendLine("}");
            sb.AppendLine();
        }

        // Generate Worker projects
        if (participantsByType.TryGetValue("Worker", out var workers))
        {
            sb.AppendLine("package \"Workers\" {");
            foreach (var worker in workers)
            {
                var projectName = SanitizeName(worker.Name);
                sb.AppendLine($"    component [{projectName}] as {worker.Alias} <<Worker>>");
            }
            sb.AppendLine("}");
            sb.AppendLine();
        }

        // Generate Microservice projects (Core, Infrastructure, Api)
        if (participantsByType.TryGetValue("Microservice", out var microservices))
        {
            sb.AppendLine("package \"Microservices\" {");
            foreach (var microservice in microservices)
            {
                var projectName = SanitizeName(microservice.Name);
                var boundedContext = projectName;

                sb.AppendLine($"    package \"{projectName}\" {{");
                sb.AppendLine($"        component [{projectName}.Api] as {microservice.Alias}_api <<WebApi>>");
                sb.AppendLine($"        component [{projectName}.Core] as {microservice.Alias}_core <<ClassLib>>");
                sb.AppendLine($"        component [{projectName}.Infrastructure] as {microservice.Alias}_infra <<ClassLib>>");
                sb.AppendLine($"        {microservice.Alias}_api --> {microservice.Alias}_infra");
                sb.AppendLine($"        {microservice.Alias}_infra --> {microservice.Alias}_core");
                sb.AppendLine("    }");
            }
            sb.AppendLine("}");
            sb.AppendLine();
        }

        // Add dependencies between components based on the original sequence diagram flow
        // Angular apps typically call APIs
        if (participantsByType.ContainsKey("Angular") || participantsByType.ContainsKey("ts"))
        {
            var angular = (participantsByType.ContainsKey("Angular") ? participantsByType["Angular"] : participantsByType["ts"]).FirstOrDefault();
            if (angular != null && participantsByType.ContainsKey("Microservice"))
            {
                var firstMicroservice = participantsByType["Microservice"].FirstOrDefault();
                if (firstMicroservice != null)
                {
                    sb.AppendLine($"{angular.Alias} --> {firstMicroservice.Alias}_api : HTTP/REST");
                }
            }
        }

        sb.AppendLine();
        sb.AppendLine("@enduml");

        return sb.ToString();
    }

    private string GenerateDefaultSolutionPlantUml(string solutionName)
    {
        var sb = new StringBuilder();

        sb.AppendLine("@startuml Domain Models");
        sb.AppendLine("!pragma layout smetana");
        sb.AppendLine("skinparam backgroundColor #FEFEFE");
        sb.AppendLine("skinparam defaultFontSize 11");
        sb.AppendLine();
        sb.AppendLine($"title {solutionName} - Domain Models");
        sb.AppendLine();
        sb.AppendLine($"package \"{solutionName}.Aggregates.Entity\" {{");
        sb.AppendLine("    class Entity <<aggregate>> {");
        sb.AppendLine("        +EntityId : string");
        sb.AppendLine("        +Name : string");
        sb.AppendLine("        +CreatedAt : DateTime");
        sb.AppendLine("        +ModifiedAt : DateTime");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine("@enduml");

        return sb.ToString();
    }

    private string SanitizeName(string name)
    {
        // Remove spaces and special characters, convert to PascalCase
        var parts = name.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return "DefaultName";
        }
        
        return string.Join("", parts.Select(p => 
        {
            if (string.IsNullOrEmpty(p)) return string.Empty;
            return char.ToUpper(p[0]) + (p.Length > 1 ? p.Substring(1).ToLower() : string.Empty);
        }));
    }
}
