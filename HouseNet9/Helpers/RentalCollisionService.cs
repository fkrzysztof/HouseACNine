using HouseNet9.Data;
using Microsoft.EntityFrameworkCore;

namespace HouseNet9.Helpers
{
    public class RentalCollisionService
    {
        private readonly ApplicationDbContext _context;

        public RentalCollisionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> HasCollisionAsync(int houseId, DateTime from, DateTime to)
        {
            return await _context.RentalHouses
                .AnyAsync(r =>
                    r.HouseId == houseId &&
                    r.IsActive &&
                    from < r.To &&
                    to > r.From);
        }
    }
}
