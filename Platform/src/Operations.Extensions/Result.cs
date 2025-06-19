// Copyright (c) ABCDEG. All rights reserved.

using FluentValidation.Results;
using OneOf;

namespace Operations.Extensions;

[GenerateOneOf]
public partial class Result<T> : OneOfBase<T, List<ValidationFailure>>
{
}
