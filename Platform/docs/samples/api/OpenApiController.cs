using Microsoft.AspNetCore.Mvc;

namespace Platform.Samples.Api;

// #region OpenApiController
/// <summary>
/// Manages user accounts and profiles
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    /// <summary>
    /// Creates a new user account
    /// </summary>
    /// <param name="request">User creation details</param>
    /// <returns>The created user with assigned ID</returns>
    /// <response code="201">User created successfully</response>
    /// <response code="400">Invalid user data provided</response>
    [HttpPost]
    [ProducesResponseType<UserResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        // Implementation here
        var user = new UserResponse(Guid.NewGuid(), request.Name, request.Email);
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    /// <summary>
    /// Retrieves a user by their unique identifier
    /// </summary>
    /// <param name="id">The user's unique identifier</param>
    /// <returns>User details if found</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser(Guid id)
    {
        // Implementation here
        return Ok(new UserResponse(id, "John Doe", "john@example.com"));
    }
}
// #endregion

// #region DataModels
/// <summary>
/// Request model for creating a new user
/// </summary>
public record CreateUserRequest(
    /// <summary>The user's full name</summary>
    string Name,
    /// <summary>The user's email address</summary>
    string Email);

/// <summary>
/// Response model containing user information
/// </summary>
public record UserResponse(
    /// <summary>The user's unique identifier</summary>
    Guid Id,
    /// <summary>The user's full name</summary>
    string Name,
    /// <summary>The user's email address</summary>
    string Email);
// #endregion