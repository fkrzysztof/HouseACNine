using HouseNet9.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/calendar")]
[ApiController]
public class CalendarController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CalendarController(ApplicationDbContext context)
    {
        _context = context;
    }

    // ===== 1. POBIERANIE ZAJĘTYCH DNI =====
    [HttpGet("reserved")]
    public async Task<IActionResult> GetReservedDates(DateTime start, DateTime end)
    {
        var reservations = await _context.RentalHouses
            .Where(r => r.From <= end && r.To >= start)
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
        if (await HasCollision(from, to))
        {
            return Conflict(new
            {
                message = "Wybrany termin jest już zajęty"
            });
        }

        int days = (to - from).Days + 1;

        //Logika naliczania cen
        decimal pricePerDay = 250;
        decimal price = days * pricePerDay;

        return Ok(new
        {
            start = from.ToString("yyyy-MM-dd"),
            end = to.ToString("yyyy-MM-dd"),
            days,
            price
        });
    }


    //Walidacja kolizji
    private async Task<bool> HasCollision(DateTime from, DateTime to)
    {
        return await _context.RentalHouses
            .AnyAsync(r =>
                r.From.Date <= to.Date &&
                r.To.Date >= from.Date
            );
    }

}

public class ReservationRequest
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
}

