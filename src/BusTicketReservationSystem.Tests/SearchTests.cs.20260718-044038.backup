using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusTicketReservationSystem.Tests
{
    using Xunit;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BusTicketReservationSystem.Application.Services;
    using BusTicketReservationSystem.Application.Contracts.Interfaces;
    using BusTicketReservationSystem.Domain.Entities;
    using BusTicketReservationSystem.Domain.Enums;

    public class SearchServiceTests
    {
        [Fact]
        public async Task SearchAvailableBusesAsync_ShouldReturnMatchingBuses_WithCorrectSeatCount()
        {
            // Arrange
            var from = "Dhaka";
            var to = "Sylhet";
            var journeyDate = DateTime.Today;

            var scheduleId = Guid.NewGuid();
            var bus = new Bus
            {
                Id = Guid.NewGuid(),
                BusName = "GreenLine",
                CompanyName = "GreenLine Ltd.",
                TotalSeats = 40,
                Price = 550
            };

            var route = new Route
            {
                FromCity = "Dhaka",
                ToCity = "Sylhet",
                BoardingPoint = "Gabtoli",
                DroppingPoint = "Amberkhana"
            };

            var schedule = new BusSchedule
            {
                Id = scheduleId,
                BusId = bus.Id,
                Bus = bus,
                Route = route,
                JourneyDate = journeyDate,
                StartTime = DateTime.Today.AddHours(8),
                ArrivalTime = DateTime.Today.AddHours(12)
            };

            var mockScheduleRepo = new Mock<IBusScheduleRepository>();
            var mockTicketRepo = new Mock<ITicketRepository>();

            mockScheduleRepo.Setup(r => r.SearchAsync(from.ToLower(), to.ToLower(), journeyDate))
                            .ReturnsAsync(new List<BusSchedule> { schedule });

            mockTicketRepo.Setup(r => r.CountByStatusAsync(scheduleId, TicketStatus.Booked)).ReturnsAsync(5);
            mockTicketRepo.Setup(r => r.CountByStatusAsync(scheduleId, TicketStatus.Sold)).ReturnsAsync(10);

            var service = new SearchService(mockScheduleRepo.Object, mockTicketRepo.Object);

            // Act
            var result = await service.SearchAvailableBusesAsync(from, to, journeyDate);

            // Assert
            Assert.Single(result);
            Assert.Equal(25, result[0].SeatsLeft); // 40 - (5 + 10)
            Assert.Equal("GreenLine", result[0].BusName);
            Assert.Equal("Sylhet", result[0].RouteTo);
        }
    }

}
