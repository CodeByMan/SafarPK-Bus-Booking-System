using BusTicketReservationSystem.Application.Contracts.DTOs;
using BusTicketReservationSystem.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace BusTicketReservationSystem.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly BookingService _booking;
        public BookingController(BookingService booking) => _booking = booking;

        [HttpGet("seatplan/{scheduleId:guid}")]
        public async Task<IActionResult> GetSeatPlan(Guid scheduleId)
        {
            if (scheduleId == Guid.Empty)
                return BadRequest(new ProblemDetails { Title = "A valid schedule ID is required." });

            var result = await _booking.GetSeatPlanAsync(scheduleId);
            return result is null
                ? NotFound(new ProblemDetails { Title = "Schedule not found." })
                : Ok(result);
        }

        [HttpPost("book")]
        public async Task<IActionResult> Book([FromBody] BookSeatInputDto input)
        {
            input.Action = "book";
            return ToActionResult(await _booking.BookSeatAsync(input));
        }

        [HttpPost("buy")]
        public async Task<IActionResult> Buy([FromBody] BookSeatInputDto input)
        {
            input.Action = "buy";
            return ToActionResult(await _booking.BookSeatAsync(input));
        }

        [HttpPost("cancel")]
        public async Task<IActionResult> Cancel([FromBody] CancelInputDto input)
        {
            return ToActionResult(await _booking.CancelAsync(input));
        }

        private IActionResult ToActionResult(BookSeatResultDto result)
        {
            return result.Code switch
            {
                BookingResultCode.Success => Ok(result),
                BookingResultCode.ValidationError => BadRequest(result),
                BookingResultCode.NotFound => NotFound(result),
                BookingResultCode.Conflict => Conflict(result),
                BookingResultCode.Forbidden => StatusCode(StatusCodes.Status403Forbidden, result),
                _ => StatusCode(StatusCodes.Status500InternalServerError, result)
            };
        }
    }
}
