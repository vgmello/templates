// Copyright (c) ABCDEG. All rights reserved.

namespace Operations.Extensions.Abstractions.Dapper;

public interface IDbParamsProvider
{
    object ToDbParams();
}
