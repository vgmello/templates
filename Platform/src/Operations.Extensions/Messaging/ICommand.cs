// Copyright (c) ABCDEG. All rights reserved.

namespace Operations.Extensions.Messaging;

// Rule: Unused type parameters should be removed
// Reason: This is a marker interface
#pragma warning disable S2326

public interface ICommand<TResult>;
