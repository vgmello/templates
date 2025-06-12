// Copyright (c) ABCDEG. All rights reserved.

using Microsoft.CodeAnalysis;

namespace Operations.Extensions.SourceGenerators;

internal static class DbCommandSourceGeneratorAnalyzers
{
    private static readonly DiagnosticDescriptor NonQueryWithGenericResultWarning = new(
        id: "DBCOMMANDGEN001",
        title: "NonQuery attribute used with generic ICommand<TResult>",
        messageFormat:
        "DbCommandAttribute's NonQuery property is true for command '{0}' which implements ICommand<{1}>. " +
        "NoQuery are only valid for ICommand<int> or ICommand<long>. " +
        "The generated handler will execute the DbConnection extensions that returns a value (Query or ExecuteScalar).",
        category: "DbCommandSourceGenerator",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor CommandMissingInterfaceError = new(
        id: "DBCOMMANDGEN002",
        title: "Command missing ICommand<TResult> interface",
        messageFormat:
        "Class '{0}' is decorated with DbCommandAttribute specifying 'sp' or 'sql' for handler generation, " +
        "but it does not implement ICommand<TResult>. Handler cannot be generated without a result type.",
        category: "DbCommandSourceGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);


    /// <summary>
    ///     If <see cref="DbCommandResultTypeInfo" /> is null, it means ICommand&lt;TResult&gt; was not found.
    ///     An error diagnostic DBCOMMANDGEN002 will be logged by ExtractTypeInfo and reported by Initialize's output action.
    ///     Generation should be skipped by the check in Initialize's RegisterSourceOutput action.
    /// </summary>
    /// <param name="typeSymbol"></param>
    /// <param name="commandResultTypeInfo"></param>
    /// <param name="dbCommandAttributeValues"></param>
    /// <param name="diagnostics"></param>
    public static void ExecuteMissingInterfaceAnalyzer(INamedTypeSymbol typeSymbol,
        DbCommandResultTypeInfo? commandResultTypeInfo, DbCommandAttributes dbCommandAttributeValues, List<Diagnostic> diagnostics)
    {
        if (commandResultTypeInfo is null && (!string.IsNullOrWhiteSpace(dbCommandAttributeValues.Sp) ||
                                              !string.IsNullOrWhiteSpace(dbCommandAttributeValues.Sql)))
        {
            var typeLocation = typeSymbol.Locations.FirstOrDefault() ?? Location.None;
            var diagnostic = Diagnostic.Create(CommandMissingInterfaceError, typeLocation, typeSymbol.Name);

            diagnostics.Add(diagnostic);
        }
    }
}
