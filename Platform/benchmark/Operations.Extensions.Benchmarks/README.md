# String Extensions Benchmark

This benchmark compares the existing `ToLowerCaseWithSeparator` implementation with an optimized version that includes a zero-allocation fast path.

## Key Optimizations

1. **Fast Path for Already-Lowercase Strings**: The optimized version first checks if any changes are needed. If the string contains no uppercase letters, it returns the original string instance without any allocations.

2. **Pre-calculated Buffer Size**: The optimized version calculates the exact buffer size needed in the first pass, then uses `string.Create` to allocate exactly the right amount of memory.

3. **Single Allocation**: Uses `string.Create` with a state tuple to avoid closure allocations.

## Test Scenarios

The benchmark tests include:
- Empty strings
- Already lowercase strings (fast path)
- Various casing patterns (PascalCase, camelCase, UPPERCASE)
- Strings with acronyms (HTTPRequest, APIName)
- Strings with numbers
- Edge cases with underscores

## Running the Benchmark

```bash
dotnet run -c Release
```

## Expected Results

The optimized implementation should show:
- **Significant performance improvement** for strings that are already in the correct format (e.g., "already_snake_case")
- **Similar or slightly better performance** for strings that need transformation
- **Reduced allocations** overall due to the fast path

## Implementation Details

### Current Implementation
- Uses `stackalloc` for buffer (stack allocation)
- Always processes the entire string
- Always creates a new string

### Optimized Implementation
- First pass: Checks if changes are needed and counts separators
- Fast path: Returns original string if no changes needed (zero allocation)
- Second pass: Only builds new string if necessary
- Uses `string.Create` for optimal allocation