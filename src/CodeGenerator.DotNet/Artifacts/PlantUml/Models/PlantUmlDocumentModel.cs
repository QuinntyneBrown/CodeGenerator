// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace CodeGenerator.DotNet.Artifacts.PlantUml.Models;

public class PlantUmlDocumentModel
{
    public PlantUmlDocumentModel()
    {
        Classes = [];
        Enums = [];
        Relationships = [];
        Packages = [];
        Components = [];
        Participants = [];
    }

    public string Title { get; set; }

    public string FilePath { get; set; }

    public List<PlantUmlClassModel> Classes { get; set; }

    public List<PlantUmlEnumModel> Enums { get; set; }

    public List<PlantUmlRelationshipModel> Relationships { get; set; }

    public List<PlantUmlPackageModel> Packages { get; set; }

    public List<PlantUmlComponentModel> Components { get; set; }

    public List<PlantUmlParticipantModel> Participants { get; set; }

    public PlantUmlMetadataModel Metadata { get; set; }
}

public class PlantUmlPackageModel
{
    public PlantUmlPackageModel()
    {
        Classes = [];
        Enums = [];
    }

    public string Name { get; set; }

    public string Stereotype { get; set; }

    public List<PlantUmlClassModel> Classes { get; set; }

    public List<PlantUmlEnumModel> Enums { get; set; }
}

public class PlantUmlComponentModel
{
    public string Name { get; set; }

    public string Alias { get; set; }

    public string Stereotype { get; set; }

    public string Note { get; set; }

    public PlantUmlCodeGeneratorSpecification CodeGeneratorSpec { get; set; }
}

public class PlantUmlCodeGeneratorSpecification
{
    public PlantUmlCodeGeneratorSpecification()
    {
        CodeGenerators = [];
    }

    public string Route { get; set; }

    public bool AuthenticationRequired { get; set; }

    public List<PlantUmlCodeGeneratorModel> CodeGenerators { get; set; }
}

public class PlantUmlCodeGeneratorModel
{
    public string HttpVerb { get; set; }

    public string Path { get; set; }

    public string Description { get; set; }

    public string RequestType { get; set; }

    public string ResponseType { get; set; }

    public string QueryParameters { get; set; }

    public string RouteParameters { get; set; }
}

public class PlantUmlMetadataModel
{
    public string GeneratedDate { get; set; }

    public string Version { get; set; }

    public string Source { get; set; }

    public string TargetFramework { get; set; }

    public string AngularVersion { get; set; }
}

public class PlantUmlParticipantModel
{
    public string Name { get; set; }

    public string Alias { get; set; }

    public string Type { get; set; }

    public string DotNetType { get; set; }
}
