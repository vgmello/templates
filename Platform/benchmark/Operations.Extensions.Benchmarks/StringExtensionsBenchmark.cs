// Copyright (c) ABCDEG. All rights reserved.

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Operations.Extensions.Abstractions.Extensions;

namespace Operations.Extensions.Benchmarks;

[SimpleJob(RuntimeMoniker.Net90)]
[MemoryDiagnoser]
[DisassemblyDiagnoser(printSource: true)]
public class StringExtensionsBenchmark
{
    private string[] _testInputs = null!;

    [GlobalSetup]
    public void Setup()
    {
        _testInputs =
        [
            "",
            "already_snake_case",
            "PascalCaseString",
            "camelCaseString",
            "UPPERCASEString",
            "StringWITHAcronym",
            "StartsWithUpper",
            "endsWithUPPER",
            "Multiple___Underscores",
            "Number1InString",
            "Number11a2InString",
            "Number11InString",
            "Number11inString",
            "StringWith1Number",
            "HTTPRequest",
            "AnotherHTTPRequestExample",
            "LeadingUnderscore",
            "MyAPI",
            "MyAPIName",
            "XXMyString",
            "A",
            "a",
            "12121_12121",
            "12121_A12121",
            "_"
        ];
    }

    [Benchmark(Baseline = true)]
    public string[] CurrentImplementation()
    {
        var results = new string[_testInputs.Length];

        for (var i = 0; i < _testInputs.Length; i++)
        {
            results[i] = ToLowerCaseWithSeparator_Current(_testInputs[i], '_');
        }

        return results;
    }

    [Benchmark]
    public string[] OptimizedImplementation()
    {
        var results = new string[_testInputs.Length];

        for (var i = 0; i < _testInputs.Length; i++)
        {
            results[i] = ToLowerCaseWithSeparator_Optimized(_testInputs[i], '_');
        }

        return results;
    }

    [Benchmark]
    public string CurrentImplementation_SingleString()
    {
        return ToLowerCaseWithSeparator_Current("AnotherHTTPRequestExample", '_');
    }

    [Benchmark]
    public string OptimizedImplementation_SingleString()
    {
        return ToLowerCaseWithSeparator_Optimized("AnotherHTTPRequestExample", '_');
    }

    [Benchmark]
    public string CurrentImplementation_NoChangesNeeded()
    {
        return ToLowerCaseWithSeparator_Current("already_snake_case", '_');
    }

    [Benchmark]
    public string OptimizedImplementation_NoChangesNeeded()
    {
        return ToLowerCaseWithSeparator_Optimized("already_snake_case", '_');
    }

    private static string ToLowerCaseWithSeparator_Current(string input, char separator)
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

    /// <summary>
    ///     Converts a string to lowercase with a separator, with a zero-allocation fast path
    ///     for strings that don't need changes.
    /// </summary>
    private static string ToLowerCaseWithSeparator_Optimized(string input, char separator)
    {
        return input.ToLowerCaseWithSeparator(separator);
    }
}
