// ValidationFilter_InvalidModel_ReturnsValidationProblem.cs
using Xunit;
using Moq;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Threading.Tasks;
using System.Collections.Generic;

public class ValidationFilter_InvalidModel_ReturnsValidationProblem
{
    [Fact]
    public async Task ValidationFilter_InvalidModel_ReturnsValidationProblem()
    {
        // Arrange
        var validator = new Mock<IValidator<CreateCashierCommand>>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<CreateCashierCommand>(), default))
            .ReturnsAsync(new ValidationResult(new[] 
            { 
                new ValidationFailure("Name", "Name is required") 
            }));

        var filter = new ValidationEndpointFilter<CreateCashierCommand>(validator.Object);
        var context = CreateMockContext(new CreateCashierCommand());

        // Act
        var result = await filter.InvokeAsync(context, _ => ValueTask.FromResult<object?>(Results.Ok()));

        // Assert
        Assert.IsType<ProblemHttpResult>(result);
    }

    private EndpointFilterInvocationContext CreateMockContext(object arg)
    {
        // Simplified mock context for demonstration
        return new EndpointFilterInvocationContext(new DefaultHttpContext(), arg);
    }
}

// Placeholders for types used in the test
public class DefaultHttpContext : HttpContext { }
public class HttpContext { }
public class EndpointFilterInvocationContext {
    public HttpContext HttpContext { get; }
    public object[] Arguments { get; }
    public EndpointFilterInvocationContext(HttpContext httpContext, params object[] arguments) {
        HttpContext = httpContext;
        Arguments = arguments;
    }
}
public delegate ValueTask<object?> EndpointFilterDelegate(EndpointFilterInvocationContext context);
public interface IEndpointFilter {
    ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next);
}
public static class Results {
    public static IResult Ok() => new OkResult();
    public static IResult ValidationProblem(IDictionary<string, string[]> errors) => new ProblemHttpResult(errors);
    public static IResult BadRequest(string detail) => new BadRequestResult();
    public static IResult Unauthorized() => new UnauthorizedResult();
    public static IResult Forbid() => new ForbidResult();
    public static IResult Problem(int statusCode, string title, string detail) => new ProblemHttpResult(statusCode, title, detail);
    public static IResult Json<T>(T data) => new JsonResult<T>(data);
}
public interface IResult { }
public class OkResult : IResult { }
public class BadRequestResult : IResult { }
public class UnauthorizedResult : IResult { }
public class ForbidResult : IResult { }
public class ProblemHttpResult : IResult {
    public ProblemHttpResult(IDictionary<string, string[]> errors) { }
    public ProblemHttpResult(int statusCode, string title, string detail) { }
}
public class JsonResult<T> : IResult {
    public JsonResult(T data) { }
}
