using System.ComponentModel.DataAnnotations;

namespace BusTicketReservationSystem.Application.Contracts.DTOs
{
    public class BookSeatInputDto
    {
        public Guid BusScheduleId { get; set; }

        [MinLength(1, ErrorMessage = "Select at least one seat.")]
        public List<Guid> SeatId { get; set; } = new();

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string PassengerName { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^03\d{9}$", ErrorMessage = "Enter a valid Pakistan mobile number in 03XXXXXXXXX format.")]
        public string PassengerMobile { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string BoardingPoint { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string DroppingPoint { get; set; } = string.Empty;

        public string Action { get; set; } = "book";
    }
}
