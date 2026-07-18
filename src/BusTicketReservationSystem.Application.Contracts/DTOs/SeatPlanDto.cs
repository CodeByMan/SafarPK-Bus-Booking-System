using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusTicketReservationSystem.Application.Contracts.DTOs
{
    public class SeatPlanDto
    {
        public Guid BusScheduleId { get; set; }
        public string BusName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string? BoardingPoint { get; set; }
        public string? DroppingPoint { get; set; }
        public List<SeatDto> Seats { get; set; } = new();
    }
}
