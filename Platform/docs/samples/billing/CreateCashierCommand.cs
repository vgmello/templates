using Operations.Extensions.Dapper;
using Operations.Extensions.Messaging;

namespace Billing.Cashier.Commands;

// <basic_command>
[DbCommand(sp: "billing.cashier_create", nonQuery: true, paramsCase: DbParamsCase.SnakeCase)]
public partial record CreateCashierCommand(
    Guid CashierId,
    string Name,
    string Email
) : ICommand<int>;
// </basic_command>