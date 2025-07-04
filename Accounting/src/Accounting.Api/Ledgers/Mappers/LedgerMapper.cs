// Copyright (c) ABCDEG. All rights reserved.

using Ledger = Accounting.Ledgers.Grpc.Models.Ledger;

namespace Accounting.Api.Ledgers.Mappers;

[Mapper]
public static partial class LedgerMapper
{
    [MapperIgnoreSource(nameof(Contracts.Ledgers.Models.Ledger.LedgerPayments))]
    public static partial Ledger ToGrpc(this Accounting.Contracts.Ledgers.Models.Ledger source);

    public static partial Ledger ToGrpc(this GetLedgersQuery.Result source);
}
