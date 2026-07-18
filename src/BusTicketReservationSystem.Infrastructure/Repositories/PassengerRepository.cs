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
    public class PassengerRepository : IPassengerRepository
    {
        private readonly AppDbContext _db;
        public PassengerRepository(AppDbContext db) => _db = db;

        public Task<Passenger?> FindByMobileAsync(string mobile) =>
            _db.Passengers.FirstOrDefaultAsync(p => p.MobileNumber == mobile);

        public async Task AddAsync(Passenger passenger) =>
            await _db.Passengers.AddAsync(passenger);
    }

}
