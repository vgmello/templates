using Operations.Extensions.Dapper;
using Operations.Extensions.Messaging;

namespace Billing.Examples;

// <keyed_datasource>
// Uses the default DbDataSource
[DbCommand(sp: "users.create_user", nonQuery: true)]
public partial record CreateUserCommand(string Name, string Email) : ICommand<int>;

// Uses a keyed DbDataSource for read operations
[DbCommand(sp: "reports.get_user_stats", dataSource: "ReportingReader")]
public partial record GetUserStatsQuery(Guid UserId) : ICommand<UserStats>;

// Uses a keyed DbDataSource for write operations
[DbCommand(sp: "audit.log_user_action", nonQuery: true, dataSource: "AuditWriter")]
public partial record LogUserActionCommand(Guid UserId, string Action) : ICommand<int>;
// </keyed_datasource>

public record UserStats(int LoginCount, DateTime LastLogin);