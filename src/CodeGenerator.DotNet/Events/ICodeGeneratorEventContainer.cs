// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using MediatR;

namespace CodeGenerator.DotNet.Events;

public interface ICodeGeneratorEventContainer
{
    IObservable<INotification> GetObservable();

    void Broadcast(INotification notification);
}
