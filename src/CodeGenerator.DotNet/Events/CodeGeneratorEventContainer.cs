// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CodeGenerator.Core.Internal;
using MediatR;

namespace CodeGenerator.DotNet.Events;

public class CodeGeneratorEventContainer : ICodeGeneratorEventContainer
{
    private readonly Observable<INotification> _observable;

    public CodeGeneratorEventContainer(Observable<INotification> observable)
    {
        _observable = observable;
    }

    public IObservable<INotification> GetObservable() => _observable;

    public void Broadcast(INotification notification) => _observable.Broadcast(notification);
}
