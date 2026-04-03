// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Artifacts;
using DotLiquid;

namespace CodeGenerator.Core.Liquid;

public class ExitTag : Tag
{
    public override void Render(DotLiquid.Context context, TextWriter result)
    {
        throw new SkipFileException();
    }
}
