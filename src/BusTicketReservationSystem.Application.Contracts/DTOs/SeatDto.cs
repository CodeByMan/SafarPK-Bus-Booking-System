namespace BusTicketReservationSystem.Application.Contracts.DTOs
{
    public class SeatDto
    {
        public Guid SeatId { get; set; }
        public Guid? TicketId { get; set; }
        public string SeatNumber { get; set; } = string.Empty;
        public int Row { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
