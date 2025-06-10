using Accounting.Contracts.Ledgers.Models;
using Accounting.Ledgers.Queries;

namespace Accounting.Ledgers.Grpc.Models;

[Riok.Mapperly.Abstractions.Mapper]
public static partial class LedgerMapper
{
    public static partial Ledger ToGrpc(this Accounting.Contracts.Ledgers.Models.Ledger source);
    public static partial Ledger ToGrpc(this GetLedgersQuery.Result source);
}
