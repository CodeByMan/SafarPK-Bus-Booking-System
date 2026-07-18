using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusTicketReservationSystem.Application.Contracts.DTOs
{
    public class AvailableBusDto
    {
        public Guid BusId { get; set; }
        public string BusName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;

        public DateTime JourneyDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public int SeatsLeft { get; set; }
        public decimal Price { get; set; }
        public Guid? BusScheduleId { get; set; }
        public string? RouteFrom { get; set; }
        public string? RouteTo { get; set; }
        public string? BoardingPoint { get; set; }
        public string? DroppingPoint { get; set; }
    }
}
