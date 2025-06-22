// Copyright (c) ABCDEG. All rights reserved.

namespace Operations.Extensions.Abstractions.Extensions;

public static class StringExtensions
{
    public static string ToSnakeCase(this string input) => ToLowerCaseWithSeparator(input, '_');

    public static string ToKebabCase(this string input) => ToLowerCaseWithSeparator(input, '-');

    public static string ToLowerCaseWithSeparator(this string input, char separator)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // Allocate enough space for the worst case (every char becomes an underscore + lower char)
        Span<char> result = stackalloc char[input.Length * 2];

        var sourceSpan = input.AsSpan();
        var resultIndex = 0;

        for (var i = 0; i < sourceSpan.Length; i++)
        {
            var currentChar = sourceSpan[i];

            // Handle digits and underscores directly
            if (!char.IsUpper(currentChar))
            {
                result[resultIndex++] = currentChar;

                continue;
            }

            // If it's the first character, convert it to lowercase directly
            if (i == 0)
            {
                result[resultIndex++] = char.ToLowerInvariant(currentChar);

                continue;
            }

            var prependUnderscore = false;

            // Ensure the last char added to result was not already an underscore
            if (result[resultIndex - 1] != separator)
            {
                var prevChar = sourceSpan[i - 1];

                if (char.IsLower(prevChar) || char.IsDigit(prevChar))
                {
                    // Current char: 'C' => "wordCap" -> "word_Cap" or "digit1Cap" -> "digit1_Cap"
                    prependUnderscore = true;
                }
                // Current char: 'W' in "ACRONYMWord" -> "ACRONYM_Word"
                // If it's part of an acronym followed (i+1) by a lowercase letter, an underscore is needed.
                else if (char.IsUpper(prevChar) && i + 1 < sourceSpan.Length)
                {
                    var nextChar = sourceSpan[i + 1];

                    // If current char is 'N' in "APIName", prevIn is 'I', nextIn is 'a'.
                    // This means 'N' starts a new word segment.
                    if (char.IsLower(nextChar) && nextChar != separator)
                    {
                        prependUnderscore = true;
                    }
                }
            }

            if (prependUnderscore && resultIndex < result.Length)
            {
                result[resultIndex++] = separator;
            }

            // Add the lowercase version of the current uppercase character
            if (resultIndex < result.Length)
            {
                result[resultIndex++] = char.ToLowerInvariant(currentChar);
            }
        }

        return new string(result[..resultIndex]);
    }
}
