// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.DotNet.Artifacts.Files;
using CodeGenerator.DotNet.Syntax.SpecFlow;

namespace CodeGenerator.DotNet.Artifacts.SpecFlow;

public class SpecFlowFeatureFileModel : CodeFileModel<SpecFlowFeatureModel>
{
    public SpecFlowFeatureFileModel(SpecFlowFeatureModel model, string directory)
        : base(model, model.Name, directory, "feature")
    {
    }
}
