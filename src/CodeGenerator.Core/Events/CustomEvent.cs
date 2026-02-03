// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace CodeGenerator.Core.Events;

public class CustomEvent<T>
{
    public CustomEvent()
    {
    }

    public string Name { get; set; }

    public T Payload { get; set; }
}
