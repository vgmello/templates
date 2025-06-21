// Copyright (c) ABCDEG. All rights reserved.

namespace Operations.ServiceDefaults.Logging;

public class DynamicLogLevelSettings
{
    public const string SectionName = "DynamicLogLevel";

    public Dictionary<string, HashSet<string>> Properties { get; set; } = [];
}
