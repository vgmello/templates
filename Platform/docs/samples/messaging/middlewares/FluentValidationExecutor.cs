using FluentValidation;
using Wolverine.Runtime.Middleware;
using Wolverine.Runtime;
using System.Threading.Tasks;

public class FluentValidationExecutor : IChainableHandler
{
    private readonly IValidatorFactory _validatorFactory;

    public FluentValidationExecutor(IValidatorFactory validatorFactory)
    {
        _validatorFactory = validatorFactory;
    }

    public async Task Handle(MessageContext context, CancellationToken cancellationToken)
    {
        var message = context.Envelope.Message;
        var validator = _validatorFactory.GetValidator(message.GetType());

        if (validator != null)
        {
            var validationContext = new ValidationContext<object>(message);
            var validationResult = await validator.ValidateAsync(validationContext, cancellationToken);

            if (!validationResult.IsValid)
            {
                Console.WriteLine($"Validation failed for message: {message.GetType().Name}");
                foreach (var error in validationResult.Errors)
                {
                    Console.WriteLine($" - {error.PropertyName}: {error.ErrorMessage}");
                }
                // Throw an exception or return a validation failure result
                return;
            }
        }

        await context.Next(cancellationToken);
    }
}