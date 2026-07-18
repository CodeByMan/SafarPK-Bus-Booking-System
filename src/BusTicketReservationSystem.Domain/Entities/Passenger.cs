using BusTicketReservationSystem.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusTicketReservationSystem.Domain.Entities
{
    public class Passenger
    {
        public Guid Id { get; set; }
        public string Name { get; private set; } = string.Empty;
        public string MobileNumber { get; private set; } = string.Empty;

        public string? BoardingPoint { get; set; }
        public string? DroppingPoint { get; set; }

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

        private Passenger() { }

        public Passenger(PassengerContact contact, string? boardingPoint = null, string? droppingPoint = null)
        {
            Name = contact.Name;
            MobileNumber = contact.MobileNumber;
            BoardingPoint = boardingPoint;
            DroppingPoint = droppingPoint;
        }

        public PassengerContact GetContact() => new(Name, MobileNumber);
    }
}
