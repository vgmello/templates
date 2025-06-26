// Copyright (c) ABCDEG. All rights reserved.

namespace Operations.Extensions.Abstractions.Messaging;

// Rule: Unused type parameters should be removed
// Reason: This is a marker interface
// ReSharper disable UnusedTypeParameter
#pragma warning disable S2326

public interface ICommand<out TResult>
{
    /// <summary>
    ///     Gets empty result
    /// </summary>
    TResult? Empty => default;
}
