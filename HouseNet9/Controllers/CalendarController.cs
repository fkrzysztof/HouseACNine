using Data.Data.HouseRentalData;
using HouseNet9.Data;
using HouseNet9.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Math;

[Route("api/calendar")]
[ApiController]
public class CalendarController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly RentalCollisionService _collisionService; 
    private readonly RentalCalculatorService _calculator;

    public CalendarController(ApplicationDbContext context, RentalCollisionService collisionService, RentalCalculatorService calculator)
    {
        _context = context;
        _collisionService = collisionService;
        _calculator = calculator;
    }





    // ===== 1. POBIERANIE ZAJĘTYCH DNI =====
    [HttpGet("reserved")]
    public async Task<IActionResult> GetReservedDates( int houseId, DateTime start, DateTime end)
    {
        var reservations = await _context.RentalHouses
            .Where(r =>
                r.HouseId == houseId &&      // 🔥 NAJWAŻNIEJSZE
                r.From <= end &&
                r.To >= start)
            .Select(r => new { r.From, r.To })
            .ToListAsync();

        var reserved = new List<string>();

        foreach (var r in reservations)
        {
            var s = r.From < start ? start : r.From;
            var e = r.To > end ? end : r.To;

            for (var d = s.Date; d <= e.Date; d = d.AddDays(1))
                reserved.Add(d.ToString("yyyy-MM-dd"));
        }

        return Ok(reserved.Distinct());
    }

    // ===== 2. LICZENIE CENY I INFO O REZERWACJI =====
    [HttpPost("calculate")]
    public async Task<IActionResult> Calculate([FromBody] ReservationRequest request)
    {
        if (request == null)
            return BadRequest();

        var from = request.From.Date;
        var to = request.To.Date;

        if (to < from)
            return BadRequest("Błędny zakres dat");

        //WALIDACJA KOLIZJI
        if (await _collisionService.HasCollisionAsync(request.HouseId, from, to))
        {
            return Conflict(new
            {
                message = "Wybrany termin jest już zajęty"
            });
        }

        //Obliczanie Ceny
        var rentalHouse = new RentalHouse
        {
            HouseId = request.HouseId,
            From = from,
            To = to,
            RentalClient = null,
            RentalStatusID = 5, // Do zapłaty
            CreationDate = DateTime.Now,
            IsActive = true
        };

        rentalHouse.ToPay = await _calculator.CalculatePriceAsync(rentalHouse, true);
        int days = rentalHouse.HowManyDaysFromSelect;
        decimal price = rentalHouse.ToPay;

        return Ok(new
        {
            start = from.ToString("yyyy-MM-dd"),
            end = to.ToString("yyyy-MM-dd"),
            days,
            price
        });
    }


}

public class ReservationRequest
{
    public int HouseId { get; set; }
    public DateTime From { get; set; }
    public DateTime To { get; set; }
}

