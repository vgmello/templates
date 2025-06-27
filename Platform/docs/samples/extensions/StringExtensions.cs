using Operations.Extensions.Abstractions.Extensions;

namespace Platform.Samples.Extensions;

// #region CaseConversion
public class NamingConventionExample
{
    public void DemonstrateCaseConversions()
    {
        // Convert C# property names to database column names
        string propertyName = "CustomerFirstName";
        string snakeCase = propertyName.ToSnakeCase();    // "customer_first_name"
        
        // Convert for URL-friendly names
        string urlName = "ProductCategoryName";
        string kebabCase = urlName.ToKebabCase();         // "product-category-name"
        
        // Handle acronyms properly
        string apiName = "XMLHttpRequest";
        string snakeApiName = apiName.ToSnakeCase();      // "xml_http_request"
        
        // Numbers are handled correctly
        string versionName = "Version2API";
        string snakeVersionName = versionName.ToSnakeCase(); // "version2_api"
    }
}
// #endregion

// #region DatabaseParameterMapping
public class DatabaseQueryBuilder
{
    public string BuildInsertQuery<T>(T entity) where T : class
    {
        var properties = typeof(T).GetProperties();
        var columns = properties.Select(p => p.Name.ToSnakeCase());
        var parameters = properties.Select(p => $"@{p.Name}");
        
        var tableName = typeof(T).Name.ToSnakeCase();
        var columnList = string.Join(", ", columns);
        var parameterList = string.Join(", ", parameters);
        
        return $"INSERT INTO {tableName} ({columnList}) VALUES ({parameterList})";
    }
}

// Example usage:
public class CustomerOrder
{
    public Guid CustomerId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; }
}

// Generated SQL would be:
// INSERT INTO customer_order (customer_id, product_name, total_amount, order_date) 
// VALUES (@CustomerId, @ProductName, @TotalAmount, @OrderDate)
// #endregion

// #region ApiNamingConventions
public class ConfigurationNaming
{
    public Dictionary<string, object> ConvertToApiFormat(object config)
    {
        var result = new Dictionary<string, object>();
        var properties = config.GetType().GetProperties();
        
        foreach (var property in properties)
        {
            // Convert C# PascalCase to kebab-case for APIs
            var apiKey = property.Name.ToKebabCase();
            var value = property.GetValue(config);
            
            if (value != null)
            {
                result[apiKey] = value;
            }
        }
        
        return result;
    }
}

// Example configuration class
public class DatabaseConfiguration
{
    public string ConnectionString { get; set; } = string.Empty;
    public int MaxPoolSize { get; set; }
    public TimeSpan CommandTimeout { get; set; }
    public bool EnableRetryOnFailure { get; set; }
}

// Converts to:
// {
//   "connection-string": "...",
//   "max-pool-size": 100,
//   "command-timeout": "00:00:30",
//   "enable-retry-on-failure": true
// }
// #endregion

// #region PerformanceOptimization
public class PerformanceExample
{
    public void DemonstratePerformanceFeatures()
    {
        // Stack allocation is used for strings under 128 characters
        string shortString = "UserName";
        string result1 = shortString.ToSnakeCase(); // Zero heap allocations
        
        // Longer strings use heap allocation but still optimized
        string longString = "VeryLongPropertyNameThatExceedsStackAllocationThreshold";
        string result2 = longString.ToSnakeCase(); // Minimal heap allocations
        
        // Bulk processing with minimal GC pressure
        var propertyNames = new[]
        {
            "FirstName", "LastName", "EmailAddress", "PhoneNumber",
            "StreetAddress", "PostalCode", "CountryCode"
        };
        
        var snakeCaseNames = propertyNames.Select(name => name.ToSnakeCase()).ToArray();
        // Efficient batch processing with predictable memory usage
    }
}
// #endregion