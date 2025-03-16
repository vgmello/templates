// Copyright (c) ABCDEG. All rights reserved.

namespace Operations.Extensions.Data;

public class Query<T>(T payload)
{
    public T Payload => payload;
}
