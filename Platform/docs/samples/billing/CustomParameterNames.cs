using Operations.Extensions.Dapper;
using Operations.Extensions.Messaging;
using System.ComponentModel.DataAnnotations.Schema;

namespace Billing.Examples;

// <column_attributes>
[DbCommand(sp: "users.create_account")]
public partial record CreateAccountCommand(
    [Column("account_id")]
    Guid Id,
    
    [Column("full_name")]
    string DisplayName,
    
    // Uses default snake_case conversion → email_address
    string EmailAddress
) : ICommand<int>;
// </column_attributes>