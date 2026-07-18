using BusTicketReservationSystem.Domain.Entities;
using BusTicketReservationSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusTicketReservationSystem.Domain.DomainService
{
    public static class SeatStateService
    {
        public static void EnsureCanBook(Seat seat)
        {
            if (seat.Status == SeatStatus.Booked || seat.Status == SeatStatus.Sold)
                throw new InvalidOperationException($"Seat {seat.SeatNumber} is not available.");
        }

        public static void Book(Seat seat) => seat.Status = SeatStatus.Booked;
        public static void Sell(Seat seat) => seat.Status = SeatStatus.Sold;
        public static void Cancel(Seat seat) => seat.Status = SeatStatus.Available;
    }

}
