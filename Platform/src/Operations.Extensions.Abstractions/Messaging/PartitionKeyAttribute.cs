// Copyright (c) ABCDEG. All rights reserved.

namespace Operations.Extensions.Abstractions.Messaging;

/// <summary>
/// Marks a property as the source for the partition key in integration events.
/// </summary>
/// <remarks>
/// Apply this attribute to a property in an integration event class to indicate
/// that its value should be used as the partition key for message routing.
/// The partition key ensures that related messages are processed in order by
/// routing them to the same partition. Only one property per class should have
/// this attribute.
/// </remarks>
/// <example>
/// <code>
/// public class OrderCreatedEvent : IIntegrationEvent
/// {
///     [PartitionKey]
///     public Guid OrderId { get; set; }
///     
///     public string GetPartitionKey() => OrderId.ToString();
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property)]
public class PartitionKeyAttribute : Attribute;
