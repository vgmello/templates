// Copyright (c) ABCDEG. All rights reserved.

namespace Operations.ServiceDefaults.Logging;

/// <summary>
/// Configuration settings for dynamic log level management.
/// </summary>
/// <remarks>
/// This class represents configuration for dynamically adjusting log levels
/// based on specific properties or conditions. It can be bound from the
/// application's configuration system.
/// </remarks>
public class DynamicLogLevelSettings
{
    /// <summary>
    /// The configuration section name where dynamic log level settings are stored.
    /// </summary>
    public const string SectionName = "DynamicLogLevel";

    /// <summary>
    /// Gets or sets the properties that control dynamic log level adjustments.
    /// </summary>
    /// <value>
    /// A dictionary where keys represent property names and values are sets of
    /// conditions or values that trigger log level changes. The exact interpretation
    /// depends on the implementation of the dynamic log level system.
    /// </value>
    public Dictionary<string, HashSet<string>> Properties { get; set; } = [];
}
