using BusTicketReservationSystem.Application.Contracts.DTOs;
using BusTicketReservationSystem.Application.Contracts.Interfaces;
using BusTicketReservationSystem.Domain.Entities;
using BusTicketReservationSystem.Domain.Enums;
using BusTicketReservationSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BusTicketReservationSystem.Infrastructure.Repositories
{
    public class BusScheduleRepository : IBusScheduleRepository
    {
        private readonly AppDbContext _db;
        public BusScheduleRepository(AppDbContext db) => _db = db;

        public Task<BusSchedule?> GetByIdAsync(Guid id) =>
            _db.BusSchedules
                .Include(s => s.Tickets)
                .Include(x => x.Bus)
                .Include(x => x.Route)
                .FirstOrDefaultAsync(x => x.Id == id);

        public async Task<IReadOnlyList<BusSchedule>> SearchAsync(string from, string to, DateTime journeyDate)
        {
            var (dayStart, dayEnd) = GetPakistanDayRangeUtc(journeyDate);

            return await _db.BusSchedules
                .AsNoTracking()
                .Include(x => x.Bus)
                .Include(x => x.Route)
                .Where(x => x.Route.FromCity.ToLower() == from &&
                            x.Route.ToCity.ToLower() == to &&
                            x.JourneyDate >= dayStart &&
                            x.JourneyDate < dayEnd)
                .OrderBy(x => x.StartTime)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<AvailableRouteDto>> GetAvailableRoutesAsync(
            DateTime journeyDate,
            int limit,
            int offset,
            string? query)
        {
            var (dayStart, dayEnd) = GetPakistanDayRangeUtc(journeyDate);
            var normalizedQuery = query?.Trim().ToLowerInvariant();

            var scheduleQuery = _db.BusSchedules
                .AsNoTracking()
                .Where(schedule => schedule.JourneyDate >= dayStart && schedule.JourneyDate < dayEnd)
                .Select(schedule => new
                {
                    From = schedule.Route.FromCity,
                    To = schedule.Route.ToCity,
                    schedule.StartTime,
                    Price = schedule.Bus.Price,
                    SeatsLeft = schedule.Bus.TotalSeats - schedule.Tickets.Count(ticket =>
                        ticket.Status == TicketStatus.Booked || ticket.Status == TicketStatus.Sold)
                })
                .Where(schedule => schedule.SeatsLeft > 0);

            if (!string.IsNullOrWhiteSpace(normalizedQuery))
            {
                scheduleQuery = scheduleQuery.Where(schedule =>
                    schedule.From.ToLower().Contains(normalizedQuery) ||
                    schedule.To.ToLower().Contains(normalizedQuery));
            }

            var schedules = await scheduleQuery.ToListAsync();

            return schedules
                .GroupBy(schedule => new { schedule.From, schedule.To })
                .Select(group => new AvailableRouteDto
                {
                    From = group.Key.From,
                    To = group.Key.To,
                    AvailableBuses = group.Count(),
                    AvailableSeats = group.Sum(schedule => schedule.SeatsLeft),
                    LowestPrice = group.Min(schedule => schedule.Price),
                    NextDeparture = group.Min(schedule => schedule.StartTime)
                })
                .OrderBy(route => route.From)
                .ThenBy(route => route.To)
                .Skip(offset)
                .Take(limit)
                .ToList();
        }
        private static (DateTime DayStartUtc, DateTime DayEndUtc) GetPakistanDayRangeUtc(DateTime date)
        {
            var timeZoneId = OperatingSystem.IsWindows()
                ? "Pakistan Standard Time"
                : "Asia/Karachi";
            var pakistanTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            var localDayStart = DateTime.SpecifyKind(date.Date, DateTimeKind.Unspecified);
            var dayStartUtc = TimeZoneInfo.ConvertTimeToUtc(localDayStart, pakistanTimeZone);
            return (dayStartUtc, dayStartUtc.AddDays(1));
        }

    }
}
