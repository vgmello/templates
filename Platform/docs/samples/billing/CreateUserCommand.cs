using Operations.Extensions.Dapper;
using Operations.Extensions.Messaging;

namespace Billing.Users.Commands;

// <handler_command>
[DbCommand(sp: "users.create_user", nonQuery: true)]
public partial record CreateUserCommand(
    string FirstName,
    string LastName,
    string Email
) : ICommand<int>;
// </handler_command>