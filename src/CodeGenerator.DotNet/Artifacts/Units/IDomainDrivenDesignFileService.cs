// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using CodeGenerator.DotNet.Syntax.Properties;

namespace CodeGenerator.DotNet.Artifacts.Units;

public interface IDomainDrivenDesignFileService
{
    Task ServiceCreateAsync(string name, string directory);

    Task MessageCreate(string name, List<PropertyModel> properties, string directory);

    Task MessageHandlerCreate(string name, string directory);

    Task ServiceBusMessageConsumerCreate(string name = "ServiceBusMessageConsumer", string messagesNamespace = null, string directory = null);
}
