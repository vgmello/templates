// Copyright (c) ABCDEG. All rights reserved.

using FluentValidation;
using FluentValidation.Results;
using JasperFx.CodeGeneration;
using JasperFx.CodeGeneration.Frames;
using JasperFx.CodeGeneration.Model;
using Operations.Extensions;
using Operations.Extensions.Messaging;
using Wolverine.Configuration;
using Wolverine.Runtime;
using Wolverine.Runtime.Handlers;
using SyncFrame = JasperFx.CodeGeneration.Frames.SyncFrame;

namespace Operations.ServiceDefaults.Messaging.Middlewares;

public class FluentValidationPolicy : IHandlerPolicy
{
    public void Apply(IReadOnlyList<HandlerChain> chains, GenerationRules rules, IServiceContainer container)
    {
        foreach (var chain in chains)
            Apply(chain, container);
    }

    private static void Apply(HandlerChain chain, IServiceContainer container)
    {
        var validatorInterface = typeof(IValidator<>).MakeGenericType(chain.MessageType);
        var registeredValidators = container.RegistrationsFor(validatorInterface);

        var methodName = registeredValidators.Count == 1 ? nameof(Executor.ExecuteOne) : nameof(Executor.ExecuteMany);

        var validationMethodInfo = typeof(Executor)
            .GetMethod(methodName)!
            .MakeGenericMethod(chain.MessageType);

        var validationMethodCall = new MethodCall(typeof(Executor), validationMethodInfo);

        chain.Middleware.Add(validationMethodCall);
        chain.Middleware.Add(new ValidationResultFrame(chain.MessageType, validationMethodCall));
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public static class Executor
    {
        public static async Task<List<ValidationFailure>> ExecuteOne<T>(IValidator<T> validator, T message)
        {
            var result = await validator.ValidateAsync(message);

            return result.Errors;
        }

        public static async Task<List<ValidationFailure>> ExecuteMany<T>(
            IEnumerable<IValidator<T>> validators, T message)
        {
            var failures = new List<ValidationFailure>();

            foreach (var validator in validators)
            {
                var result = await validator.ValidateAsync(message);

                if (result.Errors.Count is not 0)
                {
                    failures.AddRange(result.Errors);
                }
            }

            return failures;
        }
    }
}

internal class ValidationResultFrame(Type messageType, MethodCall call) : SyncFrame
{
    private Variable? _context;

    public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
    {
        static bool IsCommandOrQuery(Type i) => i.IsGenericType &&
                                                (i.GetGenericTypeDefinition() == typeof(ICommand<>) ||
                                                 i.GetGenericTypeDefinition() == typeof(IQuery<>));

        static bool HasResultTypeResponse(Type i) => i.GetGenericArguments()[0].IsGenericType &&
                                                     i.GetGenericArguments()[0].GetGenericTypeDefinition() ==
                                                     typeof(Result<>);

        var handlerResultResponseType = messageType.GetInterfaces()
            .FirstOrDefault(i => IsCommandOrQuery(i) && HasResultTypeResponse(i));

        if (handlerResultResponseType is not null)
        {
            writer.WriteComment("Evaluate whether the execution should stop based on the ValidationFailure list");

            var validationResultVariable = call.Creates.First();

            writer.Write($"BLOCK:if ({validationResultVariable.Usage}.Count is not 0)");

            var result = new CastVariable(validationResultVariable, handlerResultResponseType);

            var cascadingMessage = new CaptureCascadingMessages(result) { Target = _context! };
            cascadingMessage.GenerateCode(method, writer);

            writer.Write("return;");
            writer.FinishBlock();
        }

        Next?.GenerateCode(method, writer);
    }

    public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
    {
        _context = chain.FindVariable(typeof(MessageContext));

        yield return _context;
    }
}
