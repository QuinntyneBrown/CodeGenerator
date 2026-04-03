// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Templates;

public class TemplatePartitioner
{
    public (IReadOnlyList<TemplateFileEntry> Regular, IReadOnlyList<TemplateFileEntry> PostProcessing)
        Partition(IEnumerable<TemplateFileEntry> entries)
    {
        var regular = new List<TemplateFileEntry>();
        var post = new List<TemplateFileEntry>();

        foreach (var entry in entries)
        {
            var fileName = Path.GetFileName(entry.TemplatePath);
            if (fileName.StartsWith("_"))
                post.Add(entry);
            else
                regular.Add(entry);
        }

        regular.Sort((a, b) => string.Compare(a.OutputRelativePath, b.OutputRelativePath, StringComparison.Ordinal));
        post.Sort((a, b) => string.Compare(a.OutputRelativePath, b.OutputRelativePath, StringComparison.Ordinal));

        return (regular, post);
    }
}
