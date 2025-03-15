// Copyright (c) ABCDEG. All rights reserved.

using Microsoft.Extensions.Hosting;
using Wolverine;

namespace Operations.ServiceDefaults.Wolverine;

public static class WolverineSetupExtensions
{
    public static IHostApplicationBuilder AddWolverine(this IHostApplicationBuilder builder)
    {
        return builder.UseWolverine(o =>
        {
        });
    }
}
