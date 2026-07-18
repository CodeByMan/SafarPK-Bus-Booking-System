using BusTicketReservationSystem.Application.Contracts.DTOs;
using BusTicketReservationSystem.Application.Contracts.Interfaces;
using BusTicketReservationSystem.Domain.Enums;

namespace BusTicketReservationSystem.Application.Services
{
    public class SearchService
    {
        private readonly IBusScheduleRepository _schedules;
        private readonly ITicketRepository _tickets;

        public SearchService(IBusScheduleRepository schedules, ITicketRepository tickets)
        {
            _schedules = schedules;
            _tickets = tickets;
        }

        public async Task<List<AvailableBusDto>> SearchAvailableBusesAsync(
            string from,
            string to,
            DateTime journeyDateUtc)
        {
            var schedules = await _schedules.SearchAsync(
                from.ToLowerInvariant(),
                to.ToLowerInvariant(),
                journeyDateUtc.Date);

            var results = new List<AvailableBusDto>();
            foreach (var bs in schedules)
            {
                var booked = await _tickets.CountByStatusAsync(bs.Id, TicketStatus.Booked);
                var sold = await _tickets.CountByStatusAsync(bs.Id, TicketStatus.Sold);
                var seatsLeft = bs.Bus.TotalSeats - (booked + sold);

                if (seatsLeft <= 0)
                    continue;

                results.Add(new AvailableBusDto
                {
                    BusId = bs.BusId,
                    BusName = bs.Bus.BusName,
                    CompanyName = bs.Bus.CompanyName,
                    JourneyDate = bs.JourneyDate,
                    StartTime = bs.StartTime,
                    ArrivalTime = bs.ArrivalTime,
                    SeatsLeft = seatsLeft,
                    Price = bs.Bus.Price,
                    BusScheduleId = bs.Id,
                    RouteFrom = bs.Route.FromCity,
                    RouteTo = bs.Route.ToCity,
                    BoardingPoint = bs.Route.BoardingPoint,
                    DroppingPoint = bs.Route.DroppingPoint
                });
            }

            return results;
        }

        public Task<IReadOnlyList<AvailableRouteDto>> GetAvailableRoutesAsync(
            DateTime journeyDateUtc,
            int limit,
            int offset,
            string? query)
        {
            return _schedules.GetAvailableRoutesAsync(journeyDateUtc.Date, limit, offset, query);
        }
    }
}
