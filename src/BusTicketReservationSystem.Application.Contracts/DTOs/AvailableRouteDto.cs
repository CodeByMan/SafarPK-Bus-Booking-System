namespace BusTicketReservationSystem.Application.Contracts.DTOs
{
    public class AvailableRouteDto
    {
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public int AvailableBuses { get; set; }
        public int AvailableSeats { get; set; }
        public decimal LowestPrice { get; set; }
        public DateTime NextDeparture { get; set; }
    }
}
