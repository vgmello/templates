// Copyright (c) ABCDEG. All rights reserved.

using JasperFx.CodeGeneration;
using Wolverine.Configuration;
using Wolverine.Runtime;
using Wolverine.Runtime.Handlers;

namespace Operations.ServiceDefaults.Messaging.Middlewares;

public class ExceptionHandlingPolicy : IHandlerPolicy
{
    public void Apply(IReadOnlyList<HandlerChain> chains, GenerationRules rules, IServiceContainer container)
    {
        foreach (var middleware in chains.Select(c => c.Middleware))
        {
            if (middleware.OfType<ExceptionHandlingFrame>().Any())
                continue;

            middleware.Add(new ExceptionHandlingFrame());
        }
    }
}
