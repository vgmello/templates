using Operations.Extensions.Abstractions.Dapper;
using Operations.Extensions.Abstractions.Messaging;

[DbCommand(sp: "dbo.CreateUser", nonQuery: true)]
public record CreateUserCommand(string UserName, string Email) : ICommand<int>;