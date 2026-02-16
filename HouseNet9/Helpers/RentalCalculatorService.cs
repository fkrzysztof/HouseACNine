using Data.Data.HouseRentalData;
using HouseNet9.Data;
using Microsoft.EntityFrameworkCore;

namespace HouseNet9.Helpers
{
    public class RentalCalculatorService
    {
        private readonly ApplicationDbContext _context;

        public RentalCalculatorService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> CalculatePriceAsync(RentalHouse rental)
        {
            if (rental.RentalClientId == null)
                return 0;

            var days = rental.HowManyDaysFromSelect;
            if (days <= 0) return 0;

            var price = await _context.RentalPrices
                .Where(p => p.HouseId == rental.HouseId && p.IsActive)
                .FirstOrDefaultAsync();

            if (price == null)
                return 0;

            return days <= 7 ? price.OneWeek : price.TwoWeeks;
        }
    }


}
