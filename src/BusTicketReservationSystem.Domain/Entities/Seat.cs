using BusTicketReservationSystem.Domain.Enums;
using BusTicketReservationSystem.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusTicketReservationSystem.Domain.Entities
{
    public class Seat
    {
        public Guid Id { get; set; }
        public Guid BusId { get; set; }
        public string SeatNumber { get; private set; } = string.Empty;
        public int Row { get; private set; }
        public SeatStatus Status { get; set; } = SeatStatus.Available;

        public Bus Bus { get; set; }
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

        private Seat() { }

        public Seat(SeatPosition position)
        {
            SeatNumber = position.Number;
            Row = position.Row;
            Status = SeatStatus.Available;
        }

        public SeatPosition GetPosition() => new(SeatNumber, Row);
    }
}
