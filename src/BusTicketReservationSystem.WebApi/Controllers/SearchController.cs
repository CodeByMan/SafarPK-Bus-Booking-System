using BusTicketReservationSystem.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace BusTicketReservationSystem.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly SearchService _search;
        public SearchController(SearchService search) => _search = search;

        [HttpGet]
        public async Task<IActionResult> Search(
            [FromQuery] string from,
            [FromQuery] string to,
            [FromQuery] DateTime date)
        {
            if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
                return BadRequest(new ProblemDetails { Title = "Origin and destination are required." });

            if (from.Length > 100 || to.Length > 100)
                return BadRequest(new ProblemDetails { Title = "Origin and destination must be 100 characters or fewer." });

            if (string.Equals(from.Trim(), to.Trim(), StringComparison.OrdinalIgnoreCase))
                return BadRequest(new ProblemDetails { Title = "Origin and destination cannot be the same." });

            if (date == default)
                return BadRequest(new ProblemDetails { Title = "A valid journey date is required." });

            if (date.Date < GetPakistanToday())
                return BadRequest(new ProblemDetails { Title = "Journey date cannot be in the past." });

            var result = await _search.SearchAvailableBusesAsync(from.Trim(), to.Trim(), date.Date);
            return Ok(result);
        }

        [HttpGet("routes")]
        public async Task<IActionResult> AvailableRoutes(
            [FromQuery] DateTime date,
            [FromQuery] int limit = 24,
            [FromQuery] int offset = 0,
            [FromQuery] string? query = null)
        {
            if (date == default)
                return BadRequest(new ProblemDetails { Title = "A valid journey date is required." });

            if (date.Date < GetPakistanToday())
                return BadRequest(new ProblemDetails { Title = "Journey date cannot be in the past." });

            if (query?.Length > 100)
                return BadRequest(new ProblemDetails { Title = "Route search must be 100 characters or fewer." });

            limit = Math.Clamp(limit, 1, 100);
            offset = Math.Clamp(offset, 0, 100_000);

            var result = await _search.GetAvailableRoutesAsync(date.Date, limit, offset, query);
            return Ok(result);
        }
        private static DateTime GetPakistanToday()
        {
            var timeZoneId = OperatingSystem.IsWindows()
                ? "Pakistan Standard Time"
                : "Asia/Karachi";
            var pakistanTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, pakistanTimeZone).Date;
        }

    }
}
