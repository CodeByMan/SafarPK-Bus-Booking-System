using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusTicketReservationSystem.Domain.Entities
{
    public class Route
    {
        public Guid Id { get; set; }
        public string FromCity { get; set; } = string.Empty;
        public string ToCity { get; set; } = string.Empty;
        public double DistanceKm { get; set; }

        public string? BoardingPoint { get; set; }
        public string? DroppingPoint { get; set; }

        public ICollection<BusSchedule> BusSchedules { get; set; } = new List<BusSchedule>();
    }
}
