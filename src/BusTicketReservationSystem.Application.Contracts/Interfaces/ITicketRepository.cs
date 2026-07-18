using BusTicketReservationSystem.Domain.Entities;
using BusTicketReservationSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusTicketReservationSystem.Application.Contracts.Interfaces
{
    public interface ITicketRepository
    {
        Task<int> CountByStatusAsync(Guid scheduleId, TicketStatus status);
        Task AddAsync(Ticket ticket);
        Task<Ticket?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<Ticket>> GetByIdsAsync(IEnumerable<Guid> ids);
        Task<Ticket?> FindBySeatAndScheduleAsync(Guid seatId, Guid scheduleId);
    }

}
