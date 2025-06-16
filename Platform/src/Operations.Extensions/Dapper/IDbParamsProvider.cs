// Copyright (c) ABCDEG. All rights reserved.

namespace Operations.Extensions.Dapper;

public interface IDbParamsProvider
{
    object ToDbParams();
}
