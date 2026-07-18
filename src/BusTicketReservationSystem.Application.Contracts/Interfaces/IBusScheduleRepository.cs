using BusTicketReservationSystem.Application.Contracts.DTOs;
using BusTicketReservationSystem.Domain.Entities;

namespace BusTicketReservationSystem.Application.Contracts.Interfaces
{
    public interface IBusScheduleRepository
    {
        Task<BusSchedule?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<BusSchedule>> SearchAsync(string from, string to, DateTime journeyDate);
        Task<IReadOnlyList<AvailableRouteDto>> GetAvailableRoutesAsync(
            DateTime journeyDate,
            int limit,
            int offset,
            string? query);
    }
}
