using Operations.Extensions.Abstractions.Messaging;

namespace Platform.Subscriptions.Contracts.IntegrationEvents;

/// <summary>
/// Published when a subscription becomes active
/// </summary>
/// <param name="TenantId">Tenant identifier</param>
/// <param name="SubscriptionId">Subscription identifier</param>
/// <param name="PlanName">Subscription plan name</param>
[EventTopic<SubscriptionActivated>]
public sealed record SubscriptionActivated(
    [PartitionKey(Order = 0)] Guid TenantId,
    string SubscriptionId,
    string PlanName
);