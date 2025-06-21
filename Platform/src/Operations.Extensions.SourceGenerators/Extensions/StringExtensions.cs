// Copyright (c) ABCDEG. All rights reserved.

using System.Text;

namespace Operations.Extensions.SourceGenerators.Extensions;

public static class StringExtensions
{
    public static string ToSnakeCase(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        var sb = new StringBuilder();

        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];

            if (char.IsUpper(c))
            {
                if (i > 0 && value[i - 1] != '_') sb.Append('_');
                sb.Append(char.ToLowerInvariant(c));
            }
            else sb.Append(c);
        }

        return sb.ToString();
    }
}
