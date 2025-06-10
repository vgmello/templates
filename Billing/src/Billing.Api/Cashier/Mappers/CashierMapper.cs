using Billing.Contracts.Cashier.Models;
using Billing.Cashier.Queries;

namespace Billing.Cashier.Grpc.Models;

[Riok.Mapperly.Abstractions.Mapper]
public static partial class CashierMapper
{
    public static partial Cashier ToGrpc(this Billing.Contracts.Cashier.Models.Cashier source);
    public static partial Cashier ToGrpc(this GetCashiersQuery.Result source);
}
