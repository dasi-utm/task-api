using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;

namespace TaskManager.API.Controllers;

[ApiController]
[Route("api/v1/tasks/statistics")]
[Authorize]
public class StatisticsController : ControllerBase
{
    private readonly ITaskStatisticsService _statisticsService;

    public StatisticsController(ITaskStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
    }

    /// <summary>
    /// Gets overall task statistics (counts by status, priority, overdue).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(TaskStatisticsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatistics()
    {
        var stats = await _statisticsService.GetOverallStatisticsAsync();
        return Ok(stats);
    }

    /// <summary>
    /// Gets task statistics grouped by assigned user.
    /// </summary>
    [HttpGet("by-user")]
    [ProducesResponseType(typeof(IReadOnlyList<UserStatisticsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatisticsByUser()
    {
        var stats = await _statisticsService.GetStatisticsByUserAsync();
        return Ok(stats);
    }

    /// <summary>
    /// Gets task creation/completion timeline (last 30 days).
    /// </summary>
    [HttpGet("timeline")]
    [ProducesResponseType(typeof(IReadOnlyList<TimelineStatisticsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTimeline([FromQuery] string period = "daily")
    {
        var stats = await _statisticsService.GetTimelineStatisticsAsync(period);
        return Ok(stats);
    }
}
