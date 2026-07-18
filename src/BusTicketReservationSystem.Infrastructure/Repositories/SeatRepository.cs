using BusTicketReservationSystem.Application.Contracts.Interfaces;
using BusTicketReservationSystem.Domain.Entities;
using BusTicketReservationSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusTicketReservationSystem.Infrastructure.Repositories
{
    public class SeatRepository : ISeatRepository
    {
        private readonly AppDbContext _db;
        public SeatRepository(AppDbContext db) => _db = db;

        public async Task<IReadOnlyList<Seat>> GetByBusIdAsync(Guid busId) =>
            await _db.Seats.Where(s => s.BusId == busId).ToListAsync();

        public async Task<IReadOnlyList<Seat>> GetByIdsAsync(IEnumerable<Guid> ids) =>
            await _db.Seats.Where(s => ids.Contains(s.Id)).ToListAsync();
    }

}
