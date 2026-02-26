using Data.Data.HouseRentalData;
using HouseNet9.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace HouseNet9.Helpers
{
    public class RentalCalculatorService
    {
        private readonly ApplicationDbContext _context;

        public RentalCalculatorService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> CalculatePriceAsync(RentalHouse rental, bool? askForPrice = null)
        {
            if (rental.RentalClientId == null && askForPrice == null)
                return 0;

            if (rental.To.Date <= rental.From.Date)
                return 0;

            int nights = (rental.To.Date - rental.From.Date).Days;

            var prices = await _context.RentalPrices
                .Where(p => p.HouseId == rental.HouseId && p.IsActive)
                .OrderBy(p => p.DateTimeFrom)
                .ToListAsync();

            if (!prices.Any())
                return 0;

            decimal total = 0;

            for (var date = rental.From.Date; date < rental.To.Date; date = date.AddDays(1))
            {
                var priceForDay = prices
                    .LastOrDefault(p =>
                        (!p.DateTimeFrom.HasValue || date >= p.DateTimeFrom.Value.Date) &&
                        (!p.DateTimeTo.HasValue || date <= p.DateTimeTo.Value.Date));

                // jeśli brak ceny dla tej daty → bierzemy ostatnią ustaloną
                if (priceForDay == null)
                    priceForDay = prices.Last();

                decimal dailyPrice;

                if (nights >= 13)
                    dailyPrice = priceForDay.TwoWeeks / 13m;
                else
                    dailyPrice = priceForDay.OneWeek / 6m;

                total += dailyPrice;
            }

            return Math.Round(total, 2);
        }



    }


}
