using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusTicketReservationSystem.Domain.Entities
{
    public class BusSchedule
    {
        public Guid Id { get; set; }
        public Guid BusId { get; set; }
        public Guid RouteId { get; set; }
        public DateTime JourneyDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime ArrivalTime { get; set; }

        public Bus Bus { get; set; }
        public Route Route { get; set; }
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
