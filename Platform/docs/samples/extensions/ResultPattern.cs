using Operations.Extensions;
using FluentValidation.Results;

namespace Platform.Samples.Extensions;

// #region ResultUsage
public class UserService
{
    public async Task<Result<User>> GetUserAsync(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            return new List<ValidationFailure>
            {
                new("UserId", "User ID cannot be empty")
            };
        }

        var user = await FindUserInDatabaseAsync(userId);
        if (user is null)
        {
            return new List<ValidationFailure>
            {
                new("UserId", "User not found")
            };
        }

        return user;
    }

    private async Task<User?> FindUserInDatabaseAsync(Guid userId)
    {
        // Simulate database lookup
        await Task.Delay(10);
        return userId == Guid.Parse("550e8400-e29b-41d4-a716-446655440000") 
            ? new User(userId, "John Doe", "john@example.com")
            : null;
    }
}
// #endregion

// #region PatternMatching
public class UserController
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    public async Task<IActionResult> GetUser(Guid id)
    {
        var result = await _userService.GetUserAsync(id);

        return result.Match(
            user => Results.Ok(user),
            errors => Results.BadRequest(new { Errors = errors.Select(e => e.ErrorMessage) })
        );
    }
}
// #endregion

// #region ValidationIntegration
public class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}

public class UserRegistrationService
{
    private readonly CreateUserValidator _validator;

    public UserRegistrationService(CreateUserValidator validator)
    {
        _validator = validator;
    }

    public async Task<Result<User>> CreateUserAsync(CreateUserRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return validationResult.Errors;
        }

        // Create user logic here
        var user = new User(Guid.NewGuid(), request.Name, request.Email);
        return user;
    }
}
// #endregion

// Supporting types
public record User(Guid Id, string Name, string Email);
public record CreateUserRequest(string Name, string Email);

// Mock interfaces for compilation
public interface IActionResult { }
public static class Results
{
    public static IActionResult Ok(object value) => new OkResult();
    public static IActionResult BadRequest(object value) => new BadRequestResult();
}
public class OkResult : IActionResult { }
public class BadRequestResult : IActionResult { }

public abstract class AbstractValidator<T>
{
    protected IRuleBuilderInitial<T, TProperty> RuleFor<TProperty>(Expression<Func<T, TProperty>> expression) => null!;
    public Task<ValidationResult> ValidateAsync(T instance) => Task.FromResult(new ValidationResult());
}

public interface IRuleBuilderInitial<T, TProperty>
{
    IRuleBuilder<T, TProperty> NotEmpty();
    IRuleBuilder<T, TProperty> EmailAddress();
}

public interface IRuleBuilder<T, TProperty>
{
    IRuleBuilder<T, TProperty> MaximumLength(int length);
}

// Extension method to make Result work with pattern matching
public static class ResultExtensions
{
    public static TResult Match<T, TResult>(
        this Result<T> result,
        Func<T, TResult> onSuccess,
        Func<List<ValidationFailure>, TResult> onFailure)
    {
        if (result.IsT0)
            return onSuccess(result.AsT0);
        return onFailure(result.AsT1);
    }
}