using BusTicketReservationSystem.Application.Contracts.Interfaces;
using BusTicketReservationSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace BusTicketReservationSystem.Infrastructure.Unit_of_Work
{
    public class UnitOfWork : IUnitOfWork, IAsyncDisposable
    {
        private readonly AppDbContext _db;
        private IDbContextTransaction? _tx;

        public UnitOfWork(AppDbContext db) => _db = db;

        public async Task BeginTransactionAsync()
        {
            if (_tx is not null)
                throw new InvalidOperationException("A database transaction is already active.");

            _tx = await _db.Database.BeginTransactionAsync();
        }

        public Task SaveChangesAsync() => _db.SaveChangesAsync();

        public async Task CommitAsync()
        {
            if (_tx is null)
                return;

            try
            {
                await _tx.CommitAsync();
            }
            finally
            {
                await DisposeTransactionAsync();
            }
        }

        public async Task RollbackAsync()
        {
            if (_tx is null)
                return;

            try
            {
                await _tx.RollbackAsync();
            }
            finally
            {
                await DisposeTransactionAsync();
            }
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeTransactionAsync();
            GC.SuppressFinalize(this);
        }

        private async Task DisposeTransactionAsync()
        {
            if (_tx is null)
                return;

            await _tx.DisposeAsync();
            _tx = null;
        }
    }
}
