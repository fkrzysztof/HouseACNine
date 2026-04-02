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

            return View(comments);
        }
    }
}
