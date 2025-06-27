using Operations.Extensions.Dapper;
using Operations.Extensions.Messaging;

namespace Billing.Examples;

// <parameter_cases>
// Snake case conversion
[DbCommand(sp: "users.update_profile", paramsCase: DbParamsCase.SnakeCase)]
public partial record UpdateProfileCommand(
    string FirstName,    // → first_name
    string LastName,     // → last_name
    string EmailAddress  // → email_address
) : ICommand<int>;

// No case conversion
[DbCommand(sp: "users.update_profile", paramsCase: DbParamsCase.None)]
public partial record UpdateProfileDirectCommand(
    string FirstName,    // → FirstName
    string LastName,     // → LastName
    string EmailAddress  // → EmailAddress
) : ICommand<int>;
// </parameter_cases>