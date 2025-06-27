using Operations.Extensions.Abstractions.Dapper;
using Operations.Extensions.Abstractions.Messaging;

namespace Platform.Samples.DatabaseIntegration;

// #region StoredProcedureCommand
/// <summary>
/// Command to create a new user using stored procedure
/// </summary>
[DbCommand(sp: "user_create")]
public record CreateUserCommand(
    string Name,
    string Email,
    DateTime CreatedAt
) : ICommand<int>;
// #endregion

// #region SqlQueryCommand
/// <summary>
/// Command to find users by email pattern using SQL query
/// </summary>
[DbCommand(sql: "SELECT * FROM users WHERE email LIKE @EmailPattern")]
public record FindUsersByEmailCommand(
    string EmailPattern
) : ICommand<IEnumerable<User>>;
// #endregion

// #region FunctionCommand
/// <summary>
/// Command to get user statistics using database function
/// </summary>
[DbCommand(fn: "SELECT * FROM get_user_stats", nonQuery: false)]
public record GetUserStatsCommand(
    DateTime StartDate,
    DateTime EndDate
) : ICommand<UserStats>;
// #endregion

// #region SnakeCaseMapping
/// <summary>
/// Command with snake_case parameter mapping
/// </summary>
[DbCommand(sp: "update_user_profile", paramsCase: DbParamsCase.SnakeCase)]
public record UpdateUserProfileCommand(
    Guid UserId,           // Maps to user_id
    string FirstName,      // Maps to first_name
    string LastName,       // Maps to last_name
    DateTime LastUpdated   // Maps to last_updated
) : ICommand<int>;
// #endregion

// #region CustomColumnNames
/// <summary>
/// Command with custom column name mappings
/// </summary>
[DbCommand(sp: "create_order")]
public record CreateOrderCommand(
    [Column("customer_id")] Guid CustomerId,
    [Column("total_amount")] decimal Total,
    [Column("order_date")] DateTime CreatedDate
) : ICommand<int>;
// #endregion

// Supporting types
public record User(Guid Id, string Name, string Email);
public record UserStats(int TotalUsers, int ActiveUsers, DateTime PeriodStart, DateTime PeriodEnd);