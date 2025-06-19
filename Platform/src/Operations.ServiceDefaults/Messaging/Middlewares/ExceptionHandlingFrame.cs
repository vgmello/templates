// Copyright (c) ABCDEG. All rights reserved.

using JasperFx.CodeGeneration;
using JasperFx.CodeGeneration.Frames;
using JasperFx.CodeGeneration.Model;
using Wolverine;

namespace Operations.ServiceDefaults.Messaging.Middlewares;

public class ExceptionHandlingFrame : SyncFrame
{
    private Variable? _envelope;

    public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
    {
        writer.Write("BLOCK:try");
        Next?.GenerateCode(method, writer);
        writer.FinishBlock();

        writer.Write("BLOCK:catch (System.Exception ex)");

        writer.Write($"{_envelope?.Usage}.Failure = ex;");

        writer.Write("throw;");

        writer.FinishBlock();
    }

    public override IEnumerable<Variable> FindVariables(IMethodVariables chain)
    {
        _envelope = chain.FindVariable(typeof(Envelope));

        yield return _envelope;
    }
}
