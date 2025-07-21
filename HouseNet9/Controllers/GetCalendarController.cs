using Data.Data.HouseRentalData;
//using HouseData.Data.HouseRentalData;
using HouseNet9.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace HouseRent.Controllers
{
    public class GetCalendarController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GetCalendarController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: GetCalendar
        public async Task<IActionResult> Index(string navigation)
        {

            //Kalendarz 

            //ustawiam today
            DateTime today = DateTime.Now.Date;
            //pierwszy dzien biezacego miesiaca
            DateTime firstDay;

            //ustawiam session na aktualnie wybrany miesiac lub dzisiejsza date - aktualny miesiac
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("FirstDayofMonth")))
            {
                //jesli nie mamy zmiennej w sesji ustawiamy pierwszy dzien biezacego miesiaca ***
                firstDay = new DateTime(today.Year, today.Month, 1);
                HttpContext.Session.SetString("FirstDayofMonth", JsonConvert.SerializeObject(firstDay));
            }
            //jesli jest ustawiamy wybrany z session
            else
            {
             //   firstDay = JsonConvert.DeserializeObject<DateTime>(HttpContext.Session.GetString("FirstDayofMonth"));
                firstDay = today;    //today jest do zmiany!!!!
            }

            //Dodaje lub odejmuje miesiac NEXT - PREVIOUS oraz aktualizuje zmienna w sesji
            if (navigation != null)
            {
                if (navigation == "next")
                {
                    firstDay = firstDay.AddMonths(1);
                }
                else if (navigation == "previous")
                {
                    firstDay = firstDay.AddMonths(-1);
                }

                HttpContext.Session.SetString("FirstDayofMonth", JsonConvert.SerializeObject(firstDay));
            }

            //Ustawiamy wybrany miesiac - po ewetuanej zmianie i przed szukaniem poniedziałku
            DateTime selectedMonth = new DateTime(firstDay.Year, firstDay.Month, firstDay.Day);
            DateTime firstDayCalendarTwo = new DateTime(firstDay.Year, firstDay.Month + 1, 1);

            //cofamy do poniedzialku kalendarz 1
            while (firstDay.DayOfWeek != DayOfWeek.Monday)
            {
                firstDay = firstDay.AddDays(-1);
            }
            //cofamy do poniedzialku kalendarz 2
            while (firstDayCalendarTwo.DayOfWeek != DayOfWeek.Monday)
            {
                firstDayCalendarTwo = firstDayCalendarTwo.AddDays(-1);
            }

            DateTime calendarDay = new DateTime(firstDay.Year, firstDay.Month, firstDay.Day);
            DateTime calendarTwoDay = new DateTime(firstDayCalendarTwo.Year, firstDayCalendarTwo.Month, firstDayCalendarTwo.Day);

            //tworzmy kolekcje odpowiadajaca jednej stronie z kalendarza
            List<DateTime> calendarPage = new List<DateTime>();
            //ustawiam iteracje na 42 - 6x7 pokazuje 5 tygodni 
            for (int i = 0; i < 42; i++)
            //while (selectedMonth.Month != selectedMonth.AddMonths(1).Month)
            {
                calendarPage.Add(calendarDay);
                calendarDay = calendarDay.AddDays(1);
            }
            //tworzmy kolekcje odpowiadajaca jednej stronie z kalendarza nr 2
            List<DateTime> calendarTwoPage = new List<DateTime>();
            //ustawiam iteracje na 42 - 6x7 pokazuje 5 tygodni 
            for (int i = 0; i < 42; i++)
            //while (calendarDay.AddMonths(1).Month != selectedMonth.AddMonths(2).Month)
            {
                calendarTwoPage.Add(calendarTwoDay);
                calendarTwoDay = calendarTwoDay.AddDays(1);
            }

            //nazwy dni tygodnia
            string[] dayscOfWeek = { "Pn.", "Wt.", "Śr.", "Czw.", "Pt.", "Sb.", "Nd." };
            //nazwy miesiecy
            string[] monthOfYear = { "Styczeń", "Luty", "Marzec", "Kwiecień", "Maj", "Czerwiec", "Lipiec", "Sierpień", "Wrzesień", "Październik", "Listopad", "Grudzień" };


            ViewBag.CalendarPage = calendarPage;
            ViewBag.calendarTwoPage = calendarTwoPage;
            ViewBag.DaysOfWeek = dayscOfWeek;
            ViewBag.MonthString = monthOfYear[selectedMonth.Month - 1] + " " + selectedMonth.Year.ToString();
            ViewBag.MonthStringCalendarTwo = monthOfYear[selectedMonth.Month] + " " + selectedMonth.Year.ToString();
            ViewBag.SelectedMonth = selectedMonth.Month;

            House? house = await _context.Houses.Include(i => i.RentalHouses).FirstOrDefaultAsync();
            if(house == null)
            {
                return View("Er");
            }
            else 
            {
                return PartialView(house);
            }

            
        }

        //JS ACTION
        // POST: GetCalendar/Info
        [HttpPost]
        public async Task<IActionResult> Info([Bind("From,HouseId,HowManyDaysFromSelect")]  RentalHouse rentalHouse)
        {

                rentalHouse.To = rentalHouse.From.AddDays(rentalHouse.HowManyDaysFromSelect);
                rentalHouse.CreationDate = DateTime.Now;
                rentalHouse.IsActive = true;

                RentalPrice? rentalPrice = new RentalPrice();
                rentalPrice = await _context.RentalPrices.FirstOrDefaultAsync(f => f.HouseId == rentalHouse.HouseId);

                if (rentalPrice != null)
                {
                    if (rentalHouse.HowManyDaysFromSelect == 13)
                        rentalHouse.ToPay = rentalHouse.HowManyDaysFromSelect * rentalPrice.TwoWeeks;
                    if (rentalHouse.HowManyDaysFromSelect == 9)
                        rentalHouse.ToPay = rentalHouse.HowManyDaysFromSelect * rentalPrice.OneWeek;
                    if (rentalHouse.HowManyDaysFromSelect == 6)
                        rentalHouse.ToPay = rentalHouse.HowManyDaysFromSelect * rentalPrice.OneWeek;
                }

            HttpContext.Session.SetString("Rental", JsonConvert.SerializeObject(rentalHouse));
                
            ViewBag.NewRentalInfo = rentalHouse;
                return PartialView();
            }


        //public IActionResult Create()
        //{
        //    RentalHouse rentalHouse = new RentalHouse();

        //    rentalHouse.From = JsonConvert.DeserializeObject<DateTime>(HttpContext.Session.GetString("RentalFrom"));
        //    rentalHouse.To = JsonConvert.DeserializeObject<DateTime>(HttpContext.Session.GetString("RentalTo"));
        //    rentalHouse.ToPay = JsonConvert.DeserializeObject<decimal>(HttpContext.Session.GetString("RentalToPay"));
        //    rentalHouse.HowManyDaysFromSelect = JsonConvert.DeserializeObject<int>(HttpContext.Session.GetString("RentalHowManyDaysFromSelect"));

        //    ViewBag.NewRentalInfo = rentalHouse;
        //    return View();
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> CreateFirstStep([Bind("RentalHouseID,HouseId,From,To,RentalStatusID,CreationDate,Annotations,IsActive")] RentalHouse rentalHouse)
        //{


        //if (ModelState.IsValid)
        //{
        //    _context.Add(rentalHouse);
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}
        //ViewData["HouseId"] = new SelectList(_context.Houses, "HouseId", "HouseId", rentalHouse.HouseId);
        //ViewData["RentalStatusID"] = new SelectList(_context.RentalStatus, "RentalStatusID", "Name", rentalHouse.RentalStatusID);

        //    return View("Create");
        //}




        // POST: RentalClients/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public IActionResult Create()
        {

            //RentalHouse? rentalHouse = JsonConvert.DeserializeObject<RentalHouse>(HttpContext.Session.GetString("Rental"));
            RentalHouse rentalHouse = new RentalHouse();

            
             
            
            //var value = HttpContext.Session.GetObjectAsJson()

            //string? serializedObject = HttpContext.Session.GetString("Rental");
            //if (!string.IsNullOrEmpty(serializedObject))
            //{
            //    string obj = (string)serializedObject;
            //    RentalHouse rentalHouse = JsonSerializer.Deserialize<RentalHouse>(obj);

            //}



            //ISession session = HttpContext.Session;
            //RentalHouse? rentalHouse = null;

            //if (session.TryGetValue("Rental", out byte[] objectBytes))
            //{
            //    try
            //    {
            //        rentalHouse = JsonSerializer.Deserialize<RentalHouse>(objectBytes);
            //    }
            //    catch (JsonException)
            //    {
            //        // Obsługa błędu deserializacji
            //        Console.WriteLine("Błąd deserializacji obiektu z sesji.");
            //    }
            //}

            //if (myObject != null)
            //{
            //    // Użyj obiektu
            //    ViewBag.MyObjectName = myObject.Name;
            //}
            //else
            //{
            //    // Obsługa braku obiektu w sesji
            //    ViewBag.MyObjectName = "Brak obiektu w sesji";
            //}


            //string jsonStringFromSession = HttpContext.Session.GetString("Rental");
            //if (!string.IsNullOrEmpty(jsonStringFromSession))
            //{
            //    RentalHouse rentalHouse = JsonSerializer.Deserialize<RentalHouse>(jsonStringFromSession);
            //}


            ViewBag.NewRentalInfo = rentalHouse;
            return View();
        }

        public IActionResult ThanksForTheReservation(RentalHouse rentalHouse)
        {

            return View(rentalHouse);
        }


        // create z client
        //form POST: RentalClients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RentalClientId,Name,LastName,Email,Phone,Country,City,Street,Number,ZIPCode")] RentalClient rentalClient)
        {
            if (ModelState.IsValid)
            {
                RentalHouse? rentalHouse = new RentalHouse();
                string? rental = HttpContext.Session.GetString("Rental");

                if (string.IsNullOrEmpty(rental) != true)
                {
                    if(JsonConvert.DeserializeObject<RentalHouse>(rental) != null)
                    {
                        rentalHouse = JsonConvert.DeserializeObject<RentalHouse>(rental);
                    }
                }


                if (rentalHouse != null && await _context.RentalHouses.FirstOrDefaultAsync(f => 
                f.From.CompareTo(rentalHouse.From) <= 0 && f.To.CompareTo(rentalHouse.To) >= 0 && f.IsActive == true) == null) 
                {
                    rentalHouse.RentalStatus = await _context.RentalStatus.FirstAsync(f => f.RentalStatusID == 5 ); //do zaplaty
                    rentalHouse.RentalClient = rentalClient;
                    _context.Add(rentalHouse);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("ThanksForTheReservation", "GetCalendar", rentalHouse);
                }
                else 
                {
                    return RedirectToAction("Index");   
                }

            }
            return View(rentalClient);
        }
   
    }
}
