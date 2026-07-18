using System.Text.Json.Serialization;

namespace BusTicketReservationSystem.Application.Contracts.DTOs
{
    public class BookSeatResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<SeatDto> Seat { get; set; } = new();

        [JsonIgnore]
        public BookingResultCode Code { get; set; } = BookingResultCode.Success;
    }
}
