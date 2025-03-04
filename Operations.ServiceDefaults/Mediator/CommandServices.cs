// Copyright (c) ABCDEG. All rights reserved.

using MassTransit;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Operations.ServiceDefaults.Mediator;

/// <summary>
///     Default services required by a CommandHandler
/// </summary>
/// <remarks>
///     This class simplifies the handler unit test by allowing users to mock the services required by the handler
/// </remarks>
public interface ICommandServices
{
    IMediator Mediator { get; }
}

public class CommandServices(IServiceProvider serviceProvider) : ICommandServices
{
    private readonly Lazy<IMediator> _mediator = new(serviceProvider.GetRequiredService<IMediator>);

    public IMediator Mediator => _mediator.Value;
}

