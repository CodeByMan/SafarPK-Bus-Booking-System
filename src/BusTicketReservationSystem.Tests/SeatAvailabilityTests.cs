using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusTicketReservationSystem.Tests
{
    using Xunit;
    using System;
    using BusTicketReservationSystem.Domain.Entities;
    using BusTicketReservationSystem.Domain.Enums;
    using BusTicketReservationSystem.Domain.DomainService;
    using BusTicketReservationSystem.Domain.ValueObjects;

    public class SeatStateServiceTests
    {
        [Fact]
        public void EnsureCanBook_ShouldNotThrow_WhenSeatIsAvailable()
        {
            // Arrange
            var seat = new Seat(new SeatPosition("A1", 1))
            {
                Status = SeatStatus.Available
            };

            // Act & Assert
            var exception = Record.Exception(() => SeatStateService.EnsureCanBook(seat));
            Assert.Null(exception);
        }

        [Fact]
        public void EnsureCanBook_ShouldThrow_WhenSeatIsBooked()
        {
            // Arrange
            var seat = new Seat(new SeatPosition("A2", 1))
            {
                Status = SeatStatus.Booked
            };

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => SeatStateService.EnsureCanBook(seat));
            Assert.Contains("not available", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void EnsureCanBook_ShouldThrow_WhenSeatIsSold()
        {
            // Arrange
            var seat = new Seat(new SeatPosition("A3", 1))
            {
                Status = SeatStatus.Sold
            };

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => SeatStateService.EnsureCanBook(seat));
            Assert.Contains("not available", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Book_ShouldSetStatusToBooked()
        {
            var seat = new Seat(new SeatPosition("B1", 2));
            SeatStateService.Book(seat);
            Assert.Equal(SeatStatus.Booked, seat.Status);
        }

        [Fact]
        public void Sell_ShouldSetStatusToSold()
        {
            var seat = new Seat(new SeatPosition("B2", 2));
            SeatStateService.Sell(seat);
            Assert.Equal(SeatStatus.Sold, seat.Status);
        }

        [Fact]
        public void Cancel_ShouldSetStatusToAvailable()
        {
            var seat = new Seat(new SeatPosition("B3", 2)) { Status = SeatStatus.Booked };
            SeatStateService.Cancel(seat);
            Assert.Equal(SeatStatus.Available, seat.Status);
        }
    }

}
