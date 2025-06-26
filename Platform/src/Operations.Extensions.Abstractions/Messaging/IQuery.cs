// Copyright (c) ABCDEG. All rights reserved.

namespace Operations.Extensions.Abstractions.Messaging;

public interface IQuery<out TResult>
{
    /// <summary>
    ///     Gets empty result
    /// </summary>
    TResult? Empty => default;
}
