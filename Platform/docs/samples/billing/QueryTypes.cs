using Operations.Extensions.Dapper;
using Operations.Extensions.Messaging;

namespace Billing.Examples;

// <query_types>
// Non-query operation (INSERT/UPDATE/DELETE)
[DbCommand(sp: "users.create_user", nonQuery: true)]
public partial record CreateUserCommand(string Name, string Email) : ICommand<int>;

// Query operation returning a single value
[DbCommand(sp: "users.get_user_count", nonQuery: false)]
public partial record GetUserCountQuery() : ICommand<int>;

// Query operation returning an object
[DbCommand(sp: "users.get_user_by_id")]
public partial record GetUserQuery(Guid UserId) : ICommand<User>;

// Raw SQL query
[DbCommand(sql: "SELECT COUNT(*) FROM users WHERE active = @active")]
public partial record GetActiveUserCountQuery(bool Active) : ICommand<int>;
// </query_types>

public record User(Guid Id, string Name, string Email, bool Active);