using System.Diagnostics;

// <ContextPropagation>
Baggage.SetBaggage("tenant-id", "acme");
Activity.Current?.SetTag("tenant.id", Baggage.GetBaggage("tenant-id"));
// </ContextPropagation>
