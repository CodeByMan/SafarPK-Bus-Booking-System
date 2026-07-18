using BusTicketReservationSystem.Application.Contracts.Interfaces;
using BusTicketReservationSystem.Domain.Entities;
using BusTicketReservationSystem.Domain.Enums;
using BusTicketReservationSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BusTicketReservationSystem.Infrastructure.Repositories
{
    public class TicketRepository : ITicketRepository
    {
        private readonly AppDbContext _db;
        public TicketRepository(AppDbContext db) => _db = db;

        public async Task<int> CountByStatusAsync(Guid scheduleId, TicketStatus status) =>
            await _db.Tickets.CountAsync(t => t.BusScheduleId == scheduleId && t.Status == status);

        public async Task AddAsync(Ticket ticket) => await _db.Tickets.AddAsync(ticket);

        public Task<Ticket?> GetByIdAsync(Guid id) =>
            _db.Tickets
                .Include(t => t.Seat)
                .Include(t => t.Passenger)
                .FirstOrDefaultAsync(t => t.Id == id);

        public async Task<IReadOnlyList<Ticket>> GetByIdsAsync(IEnumerable<Guid> ids) =>
            await _db.Tickets
                .Include(t => t.Seat)
                .Include(t => t.Passenger)
                .Where(t => ids.Contains(t.Id))
                .ToListAsync();

        public async Task<Ticket?> FindBySeatAndScheduleAsync(Guid seatId, Guid scheduleId) =>
            await _db.Tickets.FirstOrDefaultAsync(t =>
                t.SeatId == seatId &&
                t.BusScheduleId == scheduleId);
    }
}
