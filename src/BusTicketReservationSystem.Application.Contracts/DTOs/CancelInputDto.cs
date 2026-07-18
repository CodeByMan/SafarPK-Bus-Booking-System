using System.ComponentModel.DataAnnotations;

namespace BusTicketReservationSystem.Application.Contracts.DTOs
{
    public class CancelInputDto
    {
        public Guid TicketId { get; set; }

        [Required]
        [RegularExpression(@"^03\d{9}$", ErrorMessage = "Enter the mobile number used for the booking.")]
        public string PassengerMobile { get; set; } = string.Empty;
    }
}
