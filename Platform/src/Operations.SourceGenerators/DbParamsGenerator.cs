using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Operations.SourceGenerators;

[Generator]
public class DbParamsGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not SyntaxReceiver receiver)
            return;

        var attributeSymbol = context.Compilation.GetTypeByMetadataName("Operations.Extensions.Dapper.DbParamsAttribute");

        if (attributeSymbol is null)
            return;

        foreach (var candidate in receiver.Candidates)
        {
            var model = context.Compilation.GetSemanticModel(candidate.SyntaxTree);

            if (model.GetDeclaredSymbol(candidate) is not INamedTypeSymbol typeSymbol)
                continue;

            if (!typeSymbol.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeSymbol)))
                continue;

            GenerateType(context, typeSymbol);
        }
    }

    private static void GenerateType(GeneratorExecutionContext context, INamedTypeSymbol symbol)
    {
        var sb = new StringBuilder();
        var ns = symbol.ContainingNamespace.IsGlobalNamespace ? null : symbol.ContainingNamespace.ToDisplayString();

        if (ns is not null)
        {
            sb.Append("namespace ").Append(ns).AppendLine(";");
            sb.AppendLine();
        }

        var containers = new Stack<INamedTypeSymbol>();
        var parent = symbol.ContainingType;

        while (parent != null)
        {
            containers.Push(parent);
            parent = parent.ContainingType;
        }

        foreach (var ct in containers)
        {
            sb.Append("partial ");
            if (ct.IsStatic) sb.Append("static ");
            sb.Append(GetTypeKeyword(ct)).Append(' ').Append(ct.Name).AppendLine();
            sb.AppendLine("{");
        }

        sb.Append("partial ");
        if (symbol.IsStatic) sb.Append("static ");
        sb.Append(GetTypeKeyword(symbol)).Append(' ').Append(symbol.Name).AppendLine();
        sb.AppendLine("{");
        sb.AppendLine("    public Dapper.DynamicParameters ToDbParams()");
        sb.AppendLine("    {");
        sb.AppendLine("        var p = new Dapper.DynamicParameters();");

        foreach (var propName in symbol.GetMembers().OfType<IPropertySymbol>().Select(p => p.Name))
        {
            var param = ToSnakeCase(propName);
            sb.Append("        p.Add(\"").Append(param).Append("\", ").Append(propName).AppendLine(");");
        }

        sb.AppendLine("        return p;");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        foreach (var _ in containers)
        {
            sb.AppendLine("}");
        }

        context.AddSource($"{symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.DbParams.g.cs", sb.ToString());
    }

    private static string GetTypeKeyword(INamedTypeSymbol symbol)
    {
        if (symbol.IsRecord)
            return symbol.IsValueType ? "record struct" : "record";

        return symbol.TypeKind switch
        {
            TypeKind.Struct => "struct",
            _ => "class",
        };
    }

    private static string ToSnakeCase(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        var sb = new StringBuilder();

        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];

            if (char.IsUpper(c))
            {
                if (i > 0) sb.Append('_');
                sb.Append(char.ToLowerInvariant(c));
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    private sealed class SyntaxReceiver : ISyntaxReceiver
    {
        public List<TypeDeclarationSyntax> Candidates { get; } = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is TypeDeclarationSyntax tds && tds.AttributeLists.Count > 0)
            {
                Candidates.Add(tds);
            }
        }
    }
}
