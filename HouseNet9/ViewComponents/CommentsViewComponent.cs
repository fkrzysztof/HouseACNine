using Microsoft.AspNetCore.Mvc;
using HouseNet9.Data;
using Microsoft.EntityFrameworkCore;

namespace HouseNet9.ViewComponents
{
    public class CommentsViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public CommentsViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int houseId, int count = 6)
        {
            // Pobranie komentarzy dla danego domu, tylko zatwierdzone
            var comments = await _context.Comments
                .Where(c => c.HouseId == houseId && c.IsApproved)
                .OrderByDescending(c => c.CreatedAt)
                .Take(count)
                .ToListAsync();

            //statystyka
                var stats = await _context.Comments
                    .Where(c => c.HouseId == houseId && c.IsApproved)
                    .GroupBy(c => c.HouseId)
                    .Select(g => new
                    {
                        Avg = g.Average(x => x.Rating),
                        Count = g.Count()
                    })
                    .FirstOrDefaultAsync();

                ViewBag.AvgRating = stats?.Avg ?? 0;
                ViewBag.TotalComments = stats?.Count ?? 0;

            string label = stats.Avg switch
            {
                >= 4.5 => "Znakomity",
                >= 4.0 => "Bardzo dobry",
                >= 3.5 => "Dobry",
                _ => "Średni"
            };

            ViewBag.RatingLabel = label;

            return View(comments);
        }
    }
}
