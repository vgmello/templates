// Copyright (c) ABCDEG. All rights reserved.

using JasperFx.Core.Reflection;
using Operations.Extensions.Abstractions.Messaging;
using Wolverine.Configuration;
using Wolverine.Runtime;
using Wolverine.Runtime.Routing;

namespace Operations.ServiceDefaults.Messaging.Wolverine;

public class IntegrationEventsConvention : IMessageRoutingConvention
{
    public void DiscoverListeners(IWolverineRuntime runtime, IReadOnlyList<Type> handledMessageTypes)
    {
        // Not worrying about this at all for this case
    }

    public IEnumerable<Endpoint> DiscoverSenders(Type messageType, IWolverineRuntime runtime)
    {
        var attribute = messageType.GetAttribute<EventTopicAttribute>();

        var defaultDomainAttribute = messageType.Assembly.GetAttribute<DefaultDomainAttribute>();

        yield break;
    }
}
