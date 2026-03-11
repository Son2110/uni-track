using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMSS.Application.DTOs.Notification;
using PMSS.Application.Interfaces.Services;

namespace PMSS.API.Controllers;

/// <summary>
/// RESTful API controller for managing notification resources
/// </summary>
[ApiController]
[Route("api/v1/notifications")]
[Produces("application/json")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>
    /// Retrieve a collection of notifications with optional filtering and pagination
    /// </summary>
    /// <param name="filterParams">Query parameters for filtering and pagination</param>
    /// <returns>A paginated collection of notifications</returns>
    /// <response code="200">Returns the list of notifications</response>
    /// <response code="400">If the filter parameters are invalid</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll([FromQuery] NotificationFilterParams filterParams)
    {
        var result = await _notificationService.GetAllNotificationsAsync(filterParams);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }

    /// <summary>
    /// Retrieve a specific notification by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the notification</param>
    /// <returns>The requested notification resource</returns>
    /// <response code="200">Returns the notification</response>
    /// <response code="404">If the notification is not found</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _notificationService.GetNotificationByIdAsync(id);
        
        if (!result.Success)
            return NotFound(result);
        
        return Ok(result);
    }

    /// <summary>
    /// Get notifications for the current authenticated user
    /// </summary>
    /// <param name="count">Maximum number of notifications to return (default: 10)</param>
    /// <returns>A list of user notifications</returns>
    /// <response code="200">Returns the user's notifications</response>
    /// <response code="401">If the user is not authenticated</response>
    [HttpGet("my-notifications")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyNotifications([FromQuery] int count = 10)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "User not authenticated" });

        var result = await _notificationService.GetUserNotificationsAsync(userId, count);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }

    /// <summary>
    /// Get unread notification count for the current authenticated user
    /// </summary>
    /// <returns>The number of unread notifications</returns>
    /// <response code="200">Returns the unread count</response>
    /// <response code="401">If the user is not authenticated</response>
    [HttpGet("unread-count")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "User not authenticated" });

        var result = await _notificationService.GetUnreadCountAsync(userId);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }

    /// <summary>
    /// Create a new notification resource
    /// </summary>
    /// <param name="dto">The notification creation data</param>
    /// <returns>The newly created notification</returns>
    /// <response code="201">Returns the newly created notification</response>
    /// <response code="400">If the request data is invalid</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateNotificationDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _notificationService.CreateNotificationAsync(dto);
        
        if (!result.Success)
            return BadRequest(result);
        
        return CreatedAtAction(nameof(GetById), new { id = result.Data?.NotificationId }, result);
    }

    /// <summary>
    /// Create multiple notifications at once
    /// </summary>
    /// <param name="dtos">The list of notification creation data</param>
    /// <returns>The newly created notifications</returns>
    /// <response code="201">Returns the newly created notifications</response>
    /// <response code="400">If the request data is invalid</response>
    [HttpPost("bulk")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateBulk([FromBody] List<CreateNotificationDto> dtos)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _notificationService.CreateBulkNotificationsAsync(dtos);
        
        if (!result.Success)
            return BadRequest(result);
        
        return CreatedAtAction(nameof(GetAll), result);
    }

    /// <summary>
    /// Mark a specific notification as read
    /// </summary>
    /// <param name="id">The unique identifier of the notification</param>
    /// <returns>Success status</returns>
    /// <response code="200">Notification marked as read successfully</response>
    /// <response code="404">If the notification is not found</response>
    [HttpPatch("{id:guid}/mark-read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var result = await _notificationService.MarkAsReadAsync(id);
        
        if (!result.Success)
            return NotFound(result);
        
        return Ok(result);
    }

    /// <summary>
    /// Mark multiple notifications as read
    /// </summary>
    /// <param name="dto">The list of notification IDs to mark as read</param>
    /// <returns>Success status</returns>
    /// <response code="200">Notifications marked as read successfully</response>
    /// <response code="400">If the request data is invalid</response>
    [HttpPatch("mark-read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> MarkMultipleAsRead([FromBody] MarkAsReadDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _notificationService.MarkMultipleAsReadAsync(dto);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }

    /// <summary>
    /// Mark all notifications as read for the current authenticated user
    /// </summary>
    /// <returns>Success status</returns>
    /// <response code="200">All notifications marked as read successfully</response>
    /// <response code="401">If the user is not authenticated</response>
    [HttpPatch("mark-all-read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "User not authenticated" });

        var result = await _notificationService.MarkAllAsReadAsync(userId);
        
        if (!result.Success)
            return BadRequest(result);
        
        return Ok(result);
    }

    /// <summary>
    /// Delete a specific notification
    /// </summary>
    /// <param name="id">The unique identifier of the notification</param>
    /// <returns>Success status</returns>
    /// <response code="200">Notification deleted successfully</response>
    /// <response code="404">If the notification is not found</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _notificationService.DeleteNotificationAsync(id);
        
        if (!result.Success)
            return NotFound(result);
        
        return Ok(result);
    }
}
