using Microsoft.AspNetCore.Mvc;
using QrGenerator.Core;

namespace HouseNet9.Controllers
{
    public class QrController : Controller
    {
        private readonly QrCodeService _qr;

        public QrController(QrCodeService qr)
        {
            _qr = qr;
        }

        public IActionResult FooterQr()
        {
            string url = $"{Request.Scheme}://{Request.Host}";

            var bytes = _qr.GeneratePng(url);

            return File(bytes, "image/png");
        }
    }
}
