// ValidationEndpointFilter.cs
using Microsoft.AspNetCore.Http;
using FluentValidation;
using FluentValidation.Results;
using System.Linq;
using System.Threading.Tasks;

public class ValidationEndpointFilter<T> : IEndpointFilter where T : class
{
    private readonly IValidator<T> _validator;

    public ValidationEndpointFilter(IValidator<T> validator)
    {
        _validator = validator;
    }

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context, 
        EndpointFilterDelegate next)
    {
        var argument = context.Arguments
            .OfType<T>()
            .FirstOrDefault();

        if (argument is not null)
        {
            var validationResult = await _validator.ValidateAsync(argument);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }
        }

        return await next(context);
    }
}

// Placeholder for FluentValidation.Results.ValidationResult.ToDictionary()
public static class ValidationResultExtensions
{
    public static IDictionary<string, string[]> ToDictionary(this ValidationResult validationResult)
    {
        return validationResult.Errors
            .GroupBy(x => x.PropertyName)
            .ToDictionary(
                x => x.Key,
                x => x.Select(y => y.ErrorMessage).ToArray());
    }
}

// Placeholder for a command
public record CreateCashierCommand();
public record UpdateCashierCommand();
