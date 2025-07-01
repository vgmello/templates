// Copyright (c) ABCDEG. All rights reserved.

using Housekeeping.Contracts.Rooms.Models;
using Housekeeping.Rooms.Commands;
using Housekeeping.Rooms.Queries;

namespace Housekeeping.Api.Rooms;

/// <summary>
///     Manages room housekeeping status and operations
/// </summary>
[ApiController]
[Route("[controller]")]
public class RoomsController(IMessageBus bus) : ControllerBase
{
    /// <summary>
    ///     Retrieves the status of a specific room
    /// </summary>
    /// <param name="id">The unique identifier of the room</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The room status details if found</returns>
    /// <response code="200" />
    /// <response code="404">If the room is not found</response>
    [HttpGet("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Room>> GetRoomStatus([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var room = await bus.InvokeQueryAsync(new GetRoomStatusQuery(id), cancellationToken);

        return room;
    }

    /// <summary>
    ///     Retrieves a list of room statuses with optional filtering
    /// </summary>
    /// <param name="query">Query parameters for filtering and pagination</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of rooms matching the specified criteria</returns>
    /// <response code="200" />
    [HttpGet("status")]
    public async Task<ActionResult<IEnumerable<GetRoomsStatusQuery.Result>>> GetRoomsStatus(
        [FromQuery] GetRoomsStatusQuery query,
        CancellationToken cancellationToken)
    {
        var rooms = await bus.InvokeQueryAsync(query, cancellationToken);

        return Ok(rooms);
    }

    /// <summary>
    ///     Updates a room's housekeeping status
    /// </summary>
    /// <param name="id">The unique identifier of the room</param>
    /// <param name="request">The status update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated room details</returns>
    /// <response code="200" />
    /// <response code="400">If the request data is invalid</response>
    /// <response code="404">If the room is not found</response>
    [HttpPut("{id:guid}/status")]
    [ProducesResponseType<object>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Room>> UpdateRoomStatus(
        [FromRoute] Guid id,
        [FromBody] UpdateRoomStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateRoomStatusCommand(id, request.Status, request.Notes, request.UpdatedBy);
        var commandResult = await bus.InvokeCommandAsync(command, cancellationToken);

        return commandResult.Match<ActionResult<Room>>(
            room => Ok(room),
            errors => BadRequest(new { Errors = errors }));
    }

    /// <summary>
    ///     Records cleaning activity for a room
    /// </summary>
    /// <param name="id">The unique identifier of the room</param>
    /// <param name="request">The cleaning record request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The cleaning record ID</returns>
    /// <response code="201" />
    /// <response code="400">If the request data is invalid</response>
    [HttpPost("{id:guid}/cleaning")]
    [ProducesResponseType<object>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CleaningRecordResponse>> RecordCleaning(
        [FromRoute] Guid id,
        [FromBody] RecordCleaningRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RecordCleaningCommand(id, request.CleanerId, request.IsComplete, request.Notes);
        var commandResult = await bus.InvokeCommandAsync(command, cancellationToken);

        return commandResult.Match(
            cleaningId => StatusCode(StatusCodes.Status201Created, new CleaningRecordResponse(cleaningId)),
            errors => BadRequest(new { Errors = errors }));
    }

    /// <summary>
    ///     Updates mini fridge usage for a room
    /// </summary>
    /// <param name="id">The unique identifier of the room</param>
    /// <param name="request">The mini fridge update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated mini fridge items</returns>
    /// <response code="200" />
    /// <response code="400">If the request data is invalid</response>
    [HttpPut("{id:guid}/mini-fridge")]
    [ProducesResponseType<object>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Dictionary<string, int>>> UpdateMiniFridge(
        [FromRoute] Guid id,
        [FromBody] UpdateMiniFridgeRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateMiniFridgeUsageCommand(id, request.Items, request.UpdatedBy);
        var commandResult = await bus.InvokeCommandAsync(command, cancellationToken);

        return commandResult.Match<ActionResult<Dictionary<string, int>>>(
            items => Ok(items),
            errors => BadRequest(new { Errors = errors }));
    }

    /// <summary>
    ///     Creates a maintenance request for a room
    /// </summary>
    /// <param name="id">The unique identifier of the room</param>
    /// <param name="request">The maintenance request details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The maintenance request ID</returns>
    /// <response code="201" />
    /// <response code="400">If the request data is invalid</response>
    [HttpPost("{id:guid}/maintenance")]
    [ProducesResponseType<object>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MaintenanceRequestResponse>> RequestMaintenance(
        [FromRoute] Guid id,
        [FromBody] MaintenanceRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RequestMaintenanceCommand(
            id,
            request.IssueType,
            request.Description,
            request.Priority,
            request.ReportedBy);

        var commandResult = await bus.InvokeCommandAsync(command, cancellationToken);

        return commandResult.Match(
            requestId => StatusCode(StatusCodes.Status201Created, new MaintenanceRequestResponse(requestId)),
            errors => BadRequest(new { Errors = errors }));
    }
}
