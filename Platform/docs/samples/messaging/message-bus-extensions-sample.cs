namespace SampleApp
{
    using Operations.Extensions.Abstractions.Messaging;
    using Operations.Extensions.Messaging;
    using Wolverine;
    using System.Threading.Tasks;
    using System.Threading;

    public partial class ApplicationService
    {
        // <InvokeCommandExample>
        public async Task<int> CreateUser(string userName, string email)
        {
            var command = new CreateUserCommand(userName, email);
            var userId = await _messageBus.InvokeCommandAsync(command);
            return userId;
        }
        // </InvokeCommandExample>

        // <InvokeQueryExample>
        public async Task<User> GetUser(int userId)
        {
            var query = new GetUserByIdQuery(userId);
            var user = await _messageBus.InvokeQueryAsync(query);
            return user;
        }
        // </InvokeQueryExample>
    }

    public record CreateUserCommand(string UserName, string Email) : ICommand<int>;
    public record GetUserByIdQuery(int UserId) : IQuery<User>;

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}