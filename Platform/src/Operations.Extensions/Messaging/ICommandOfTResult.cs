// Copyright (c) ABCDEG. All rights reserved.

namespace Operations.Extensions.Messaging;

/// <summary>
/// Represents a command that produces a result of type <typeparamref name="TResult"/>.
/// </summary>
/// <typeparam name="TResult">The type of the result produced by the command.</typeparam>
public interface ICommand<TResult> // Removed ": ICommand"
{
}
