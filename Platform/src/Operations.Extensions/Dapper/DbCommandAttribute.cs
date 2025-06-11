namespace Operations.Extensions.Dapper;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class DbCommandAttribute : Attribute
{
    public string CommandText { get; }
    public bool UseStoredProcedure { get; }

    private static bool DefaultUseStoredProcedure =>
#if OPERATIONS_DB_USE_TEXT
        false;
#else
        true;
#endif

    public DbCommandAttribute(string commandText)
        : this(commandText, null)
    {
    }

    public DbCommandAttribute(string commandText, bool? useStoredProcedure)
    {
        CommandText = commandText;
        UseStoredProcedure = useStoredProcedure ?? DefaultUseStoredProcedure;
    }
}
