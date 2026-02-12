using Microsoft.AspNetCore.Mvc;

namespace HouseNet9.ViewComponents
{
    public class HouseTabsViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(int houseId)
        {
            ViewBag.HouseId = houseId;
            return View();
        }
    }
}
