using BusTicketReservationSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusTicketReservationSystem.Domain.Entities
{
    public class Ticket
    {
        public Guid Id { get; set; }
        public Guid SeatId { get; set; }
        public Guid PassengerId { get; set; }
        public Guid BusScheduleId { get; set; }
        public TicketStatus Status { get; set; } = TicketStatus.Pending;

        public string? BoardingPoint { get; set; }
        public string? DroppingPoint { get; set; }

        public Seat Seat { get; set; }
        public Passenger Passenger { get; set; }
        public BusSchedule BusSchedule { get; set; }
    }
}
