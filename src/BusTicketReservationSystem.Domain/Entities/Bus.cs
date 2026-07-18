using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusTicketReservationSystem.Domain.Entities
{
    public class Bus
    {
        public Guid Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string BusName { get; set; } = string.Empty;
        public int TotalSeats { get; set; }
        public decimal Price { get; set; }

        public ICollection<Seat> Seats { get; set; } = new List<Seat>();
        public ICollection<BusSchedule> Schedules { get; set; } = new List<BusSchedule>();
    }
}
