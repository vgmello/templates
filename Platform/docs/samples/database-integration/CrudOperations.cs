using Operations.Extensions.Dapper;
using Operations.Extensions.Abstractions.Dapper;
using System.Data;

// <CrudOperationsSetup>
// Basic CRUD operations using the Platform's Dapper extensions
public class UserRepository
{
    private readonly IDbDataSource _dataSource;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(IDbDataSource dataSource, ILogger<UserRepository> logger)
    {
        _dataSource = dataSource;
        _logger = logger;
    }

    // <CreateOperation>
    public async Task<User> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating user with email {Email}", request.Email);

        using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        const string sql = """
            INSERT INTO users (id, email, first_name, last_name, created_at, updated_at)
            VALUES (@Id, @Email, @FirstName, @LastName, @CreatedAt, @UpdatedAt)
            """;

        await connection.ExecuteAsync(sql, user);

        _logger.LogInformation("User {UserId} created successfully", user.Id);
        return user;
    }
    // </CreateOperation>

    // <ReadOperation>
    public async Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving user {UserId}", userId);

        using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        const string sql = """
            SELECT id, email, first_name, last_name, created_at, updated_at
            FROM users 
            WHERE id = @UserId
            """;

        var user = await connection.QuerySingleOrDefaultAsync<User>(sql, new { UserId = userId });

        if (user != null)
        {
            _logger.LogDebug("User {UserId} found", userId);
        }
        else
        {
            _logger.LogDebug("User {UserId} not found", userId);
        }

        return user;
    }

    public async Task<IEnumerable<User>> GetUsersByEmailDomainAsync(string domain, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving users with email domain {Domain}", domain);

        using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        const string sql = """
            SELECT id, email, first_name, last_name, created_at, updated_at
            FROM users 
            WHERE email LIKE @EmailPattern
            ORDER BY created_at DESC
            """;

        var users = await connection.QueryAsync<User>(sql, new { EmailPattern = $"%@{domain}" });

        _logger.LogInformation("Found {UserCount} users with domain {Domain}", users.Count(), domain);
        return users;
    }
    // </ReadOperation>

    // <UpdateOperation>
    public async Task<User?> UpdateUserAsync(Guid userId, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating user {UserId}", userId);

        using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            // First, get the current user
            const string selectSql = """
                SELECT id, email, first_name, last_name, created_at, updated_at
                FROM users 
                WHERE id = @UserId
                """;

            var existingUser = await connection.QuerySingleOrDefaultAsync<User>(selectSql, new { UserId = userId }, transaction);

            if (existingUser == null)
            {
                _logger.LogWarning("User {UserId} not found for update", userId);
                return null;
            }

            // Update the user
            const string updateSql = """
                UPDATE users 
                SET first_name = @FirstName, 
                    last_name = @LastName, 
                    updated_at = @UpdatedAt
                WHERE id = @UserId
                """;

            var updatedUser = existingUser with
            {
                FirstName = request.FirstName ?? existingUser.FirstName,
                LastName = request.LastName ?? existingUser.LastName,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            var rowsAffected = await connection.ExecuteAsync(updateSql, updatedUser, transaction);

            if (rowsAffected == 0)
            {
                _logger.LogWarning("No rows affected when updating user {UserId}", userId);
                await transaction.RollbackAsync(cancellationToken);
                return null;
            }

            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("User {UserId} updated successfully", userId);
            return updatedUser;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", userId);
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
    // </UpdateOperation>

    // <DeleteOperation>
    public async Task<bool> DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting user {UserId}", userId);

        using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        const string sql = """
            DELETE FROM users 
            WHERE id = @UserId
            """;

        var rowsAffected = await connection.ExecuteAsync(sql, new { UserId = userId });

        if (rowsAffected > 0)
        {
            _logger.LogInformation("User {UserId} deleted successfully", userId);
            return true;
        }

        _logger.LogWarning("User {UserId} not found for deletion", userId);
        return false;
    }
    // </DeleteOperation>

    // <BulkOperations>
    public async Task<int> CreateUsersAsync(IEnumerable<CreateUserRequest> requests, CancellationToken cancellationToken = default)
    {
        var users = requests.Select(request => new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        }).ToList();

        _logger.LogInformation("Creating {UserCount} users in bulk", users.Count);

        using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);

        const string sql = """
            INSERT INTO users (id, email, first_name, last_name, created_at, updated_at)
            VALUES (@Id, @Email, @FirstName, @LastName, @CreatedAt, @UpdatedAt)
            """;

        var rowsAffected = await connection.ExecuteAsync(sql, users);

        _logger.LogInformation("Successfully created {RowsAffected} users", rowsAffected);
        return rowsAffected;
    }
    // </BulkOperations>
}
// </CrudOperationsSetup>

// <DataModels>
public record User
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public record CreateUserRequest(
    string Email,
    string FirstName,
    string LastName);

public record UpdateUserRequest(
    string? FirstName = null,
    string? LastName = null);
// </DataModels>