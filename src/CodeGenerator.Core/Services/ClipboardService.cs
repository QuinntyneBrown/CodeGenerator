// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Services;

public class ClipboardService : IClipboardService
{
    public void SetText(string value)
    {
        TextCopy.ClipboardService.SetText(value);
    }

    public async Task<string> GetTextAsync(CancellationToken cancellationToken = default)
    {
        return await TextCopy.ClipboardService.GetTextAsync(cancellationToken);
    }
}
