using Data.Data.HouseRentalData;
//using HouseData.Data.HouseRentalData;
using HouseNet9.Data;
using Mail;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Globalization;

namespace HouseRent.Controllers
{
    public class GetCalendarController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public GetCalendarController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        //public async Task<IActionResult> GetCalendarNew()
        //{
        //    var reservations = await _context.RentalHouses
        //        .Select(r => new { r.From, r.To })
        //        .ToListAsync();

        //    var reservedDates = new List<string>();

        //    foreach (var r in reservations)
        //    {
        //        for (var d = r.From.Date; d <= r.To.Date; d = d.AddDays(1))
        //        {
        //            reservedDates.Add(d.ToString("yyyy-MM-dd"));
        //        }
        //    }

        //    // Przekazujemy jako model PartialView
        //    return PartialView("Calendar", reservedDates.Distinct().ToList());
        //}



        // Pobiera zajęte dni w zadanym zakresie
        //[HttpGet("reserved")]
        public async Task<IActionResult> GetReservedDates(DateTime start, DateTime end)
        {
            var reservations = await _context.RentalHouses
                .Where(r => r.From <= end && r.To >= start)
                .Select(r => new { r.From, r.To })
                .ToListAsync();

            var reservedDates = new List<string>();
            foreach (var r in reservations)
            {
                var s = r.From < start ? start : r.From;
                var e = r.To > end ? end : r.To;
                for (var d = s.Date; d <= e.Date; d = d.AddDays(1))
                {
                    reservedDates.Add(d.ToString("yyyy-MM-dd"));
                }
            }

            return Ok(reservedDates.Distinct());
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
                //jesli nie mamy zmiennej w sesji ustawiamy pierwszy dzien biezacego miesiaca
                firstDay = new DateTime(today.Year, today.Month, 1);
                HttpContext.Session.SetString("FirstDayofMonth", JsonConvert.SerializeObject(firstDay));
            }
            //jesli jest ustawiamy wybrany z session
            else
            {
                firstDay = JsonConvert.DeserializeObject<DateTime>(HttpContext.Session.GetString("FirstDayofMonth"));
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
                
                //zapisuje zmiany, Aktualizacja session
                HttpContext.Session.SetString("FirstDayofMonth", JsonConvert.SerializeObject(firstDay));
            }

            //czy te zmienne sa potrzebne? do prawidłowego wskazania miesiaca.??

            

            //Ustawiamy wybrany miesiac - po ewetuanej zmianie i przed szukaniem poniedziałku

            //dodaje miesiac aby wyswietli go na kal 1
            DateTime selectedMonth = new DateTime(firstDay.Year, firstDay.Month, firstDay.Day);
            //dodaje miesiac aby wyswietli go na kal 2
            DateTime firstDayCalendarTwo = new DateTime(firstDay.AddMonths(1).Year, firstDay.AddMonths(1).Month, 1);

            //firstDay  - nie oznacza 1 pokazanego ale 1 poniedzialek który moze byc z miesiaca poprzedzajacego i byc niewidoczny
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
            {
                calendarPage.Add(calendarDay);
                calendarDay = calendarDay.AddDays(1);
            }
            //tworzmy kolekcje odpowiadajaca jednej stronie z kalendarza nr 2
            List<DateTime> calendarTwoPage = new List<DateTime>();
            //ustawiam iteracje na 42 - 6x7 pokazuje 5 tygodni 
            for (int i = 0; i < 42; i++)
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
            //tu jest problem z nowym rokiem !!!!!!!!!!!!!!!!!
            //ViewBag.MonthStringCalendarTwo = monthOfYear[selectedMonth.Month] + " " + selectedMonth.Year.ToString();
            ViewBag.MonthStringCalendarTwo = monthOfYear[selectedMonth.AddMonths(1).Month -1] + " " + selectedMonth.Year.ToString();
            
            //zmiana na calego dataTime (potrzebny do przeskakiwania lat gru-sty)
            ViewBag.SelectedMonth = selectedMonth;
            //ViewBag.SelectedMonth = selectedMonth.Month;

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

            RentalHouse? rentalHouse = JsonConvert.DeserializeObject<RentalHouse>(HttpContext.Session.GetString("Rental"));
           // RentalHouse rentalHouse = new RentalHouse();

            
             
            
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

        //REALIZACJA REZERWACJI
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


                if (rentalHouse != null && await _context.RentalHouses.Include(i => i.House).FirstOrDefaultAsync(f => 
                f.From.CompareTo(rentalHouse.From) <= 0 && f.To.CompareTo(rentalHouse.To) >= 0 && f.IsActive == true) == null) 
                {
                    rentalHouse.RentalStatus = await _context.RentalStatus.FirstAsync(f => f.RentalStatusID == 5 ); //do zaplaty
                    rentalHouse.RentalClient = rentalClient;
                    _context.Add(rentalHouse);
                    await _context.SaveChangesAsync();

                    var houseThis = _context.Houses
                        .Where(w => w.HouseId == rentalHouse.HouseId)
                        .Include(i => i.Contacts)
                            .ThenInclude(i => i.Addresses)
                         .Include(i => i.Contacts)
                            .ThenInclude(i => i.PhoneNumbers)
                         .Include(i => i.Contacts)
                            .ThenInclude(i => i.EmailAddresses)
                        .AsSplitQuery()
                        .FirstOrDefault();

                    ////wysyłanie maila
                    //// Obliczenia
                    //decimal zadatek = rentalHouse.ToPay * 0.3m;
                    //DateTime zadatekTermin = DateTime.Now.AddDays(7);
                    //DateTime pelnaKwotaTermin = rentalHouse.From.AddDays(-30);

                    //string mailBody = $@"
                    //                    <html>
                    //                    <body style='font-family: Arial, sans-serif; font-size:14px; color:#000000;'>

                    //                    <h2 style='color:#000000;'>Nowa rezerwacja domu</h2>
                    //                    <hr style='border:0; border-top:1px dashed #aaa;'/>

                    //                    <p>
                    //                        <span style='font-size:16px; font-weight:bold; color:#000000;'>Dom:</span> 
                    //                        <span style='font-size:20px; margin-left:6px; font-weight:bold; color:#000000;'>{houseThis?.Name}</span>
                    //                    </p>

                    //                    <p>
                    //                        <span style='font-size:16px; font-weight:bold; color:#000000;'>Termin:</span> <br>
                    //                        <span style='font-size:20px; margin-left:6px; font-weight:bold; color:#000000;'>
                    //                            {rentalHouse.From:dd.MM.yyyy} - {rentalHouse.To:dd.MM.yyyy}</span> 
                    //                        <span style='font-size:16px; font-weight:bold; color:#000000;'>({rentalHouse.HowManyDaysFromSelect} dni)
                    //                        </span>
                    //                    </p>

                    //                    <div style=""border:2px solid #000;padding: 0px;margin: 25px 0;background:#fff;font-family:Arial,sans-serif;font-size:20px;font-weight:bold;"">

                    //                      <p style=""font-size:20px;font-weight:bold;color:#000;padding: 30px;"">
                    //                        Do zapłaty (łącznie): 
                    //                        <span style=""font-size:22px; font-weight:bold; color:#000;"">
                    //                          {rentalHouse.ToPay.ToString("N2", CultureInfo.InvariantCulture)} €
                    //                        </span>
                    //                      </p>

                    //                      <table style=""width:100%; border-collapse:collapse; text-align:center; font-size:20px; font-weight:bold; color:#000;"">
                    //                        <thead>
                    //                          <tr style=""background:#f2f2f2;color: #000000;"">
                    //                            <th style=""padding: 20px 5px;border-bottom:0px solid #000;"">Zadatek (30%)</th>
                    //                            <th style=""padding: 20px 5px;border-bottom:0px solid #000;"">Pozostała kwota</th>
                    //                          </tr>
                    //                        </thead>
                    //                        <tbody>
                    //                          <tr>
                    //                            <td style=""padding: 20px 5px;border-bottom:1px solid #000;"">
                    //                              {zadatek.ToString("N2", CultureInfo.InvariantCulture)} €
                    //                            </td>
                    //                            <td style=""padding: 20px 5px;border-bottom:1px solid #000;"">
                    //                              {(rentalHouse.ToPay - zadatek).ToString("N2", CultureInfo.InvariantCulture)} €
                    //                            </td>
                    //                          </tr>
                    //                          <tr style=""background:#f9f9f9;"">
                    //                            <td style=""padding: 20px 5px;border-bottom:1px solid #000;"">Termin płatności</td>
                    //                            <td style=""padding: 20px 5px;border-bottom:1px solid #000;"">Termin płatności</td>
                    //                          </tr>
                    //                          <tr>
                    //                            <td style=""padding: 20px 5px;border-bottom:0px solid #000; color: red;"">{zadatekTermin:dd.MM.yyyy}</td>
                    //                            <td style=""padding: 20px 5px;border-bottom:0px solid #000; color: red;"">{pelnaKwotaTermin:dd.MM.yyyy}</td>
                    //                          </tr>
                    //                        </tbody>
                    //                      </table>
                    //                    </div>
                    //                    <hr style='border:1px solid #ccc;'/>

                    //                    <h3>Klient:</h3>
                    //                    <p>
                    //                        <b>{rentalClient.FullName}</b><br/>
                    //                        {rentalClient.Street} {rentalClient.Number}<br/>
                    //                        {rentalClient.ZIPCode} {rentalClient.City}, {rentalClient.Country}<br/>
                    //                        Telefon: {rentalClient.Phone}<br/>
                    //                        Email: {rentalClient.Email}
                    //                    </p>

                    //                    ";



                    //// Kontakty domu
                    //if (houseThis?.Contacts != null && houseThis.Contacts.Any())
                    //{
                    //    mailBody += "<h3>Kontakty do domu:</h3>";

                    //    foreach (var contact in houseThis.Contacts)
                    //    {
                    //        mailBody += $@"
                    //        <p style='margin-left:10px; color:#000000;'>
                    //            <b>{contact.Name}</b><br/>
                    //        ";

                    //        if (contact.Addresses != null && contact.Addresses.Any())
                    //        {
                    //            foreach (var addr in contact.Addresses)
                    //            {
                    //                mailBody += $@"
                    //                {addr.Street}<br/>
                    //                {addr.PostalCode} {addr.City}, {addr.Country}<br/>
                    //                ";
                    //            }
                    //        }

                    //        if (contact.PhoneNumbers != null && contact.PhoneNumbers.Any())
                    //        {
                    //            foreach (var phone in contact.PhoneNumbers)
                    //            {
                    //                mailBody += $"Telefon: {phone.Number}<br/>";
                    //            }
                    //        }

                    //        if (contact.EmailAddresses != null && contact.EmailAddresses.Any())
                    //        {
                    //            foreach (var email in contact.EmailAddresses)
                    //            {
                    //                mailBody += $"Email: {email.Email}<br/>";
                    //            }
                    //        }

                    //        mailBody += "</p>";
                    //    }
                    //}

                    //mailBody += $@"
                    //            <br/><br/><hr/>
                    //            {houseThis?.RentalRules}
                    //            <br/><br/>
                    //            </body>
                    //            </html>
                    //            ";


                    // Obliczenia
                    //decimal zadatek = rentalHouse.ToPay * 0.3m;
                    //DateTime zadatekTermin = DateTime.Now.AddDays(7);
                    //DateTime pelnaKwotaTermin = rentalHouse.From.AddDays(-30);

                    //string mailBody = $@"
                    //    <html>
                    //    <body style='font-family: Arial, sans-serif; font-size:14px; color:#000000;'>

                    //    <h2 style='color:#000000;'>Nowa rezerwacja domu</h2>
                    //    <hr style='border:0; border-top:1px dashed #aaa;'/>

                    //    <table style='width:100%; border-collapse:collapse; border:2px solid #000; font-size:16px; color:#000;'>
                    //      <tbody>
                    //        <!-- Informacje o domu -->
                    //        <tr style='background:#f2f2f2;'>
                    //          <td style='padding:10px; font-weight:bold; width:200px;'>Dom</td>
                    //          <td style='padding:10px;' colspan='3'>{houseThis?.Name}</td>
                    //        </tr>

                    //        <!-- Termin rezerwacji -->
                    //        <tr>
                    //          <td style='padding:10px; font-weight:bold;'>Termin</td>
                    //          <td style='padding:10px;' colspan='3'>{rentalHouse.From:dd.MM.yyyy} - {rentalHouse.To:dd.MM.yyyy} ({rentalHouse.HowManyDaysFromSelect} dni)</td>
                    //        </tr>

                    //        <!-- Kwoty -->
                    //        <tr style='background:#f2f2f2; text-align:center;'>
                    //          <td style='padding:10px; font-weight:bold;'>Do zapłaty (łącznie)</td>
                    //          <td style='padding:10px; font-weight:bold;'>Zadatek (30%)</td>
                    //          <td style='padding:10px; font-weight:bold;'>Pozostała kwota</td>
                    //          <td style='padding:10px; font-weight:bold;'>Terminy płatności</td>
                    //        </tr>
                    //        <tr style='text-align:center;'>
                    //          <td style='padding:10px; font-weight:bold;'>{rentalHouse.ToPay.ToString("N2", CultureInfo.InvariantCulture)} €</td>
                    //          <td style='padding:10px; font-weight:bold;'>{zadatek.ToString("N2", CultureInfo.InvariantCulture)} €</td>
                    //          <td style='padding:10px; font-weight:bold;'>{(rentalHouse.ToPay - zadatek).ToString("N2", CultureInfo.InvariantCulture)} €</td>
                    //          <td style='padding:10px;'>{zadatekTermin:dd.MM.yyyy} / {pelnaKwotaTermin:dd.MM.yyyy}</td>
                    //        </tr>

                    //        <!-- Informacje o kliencie -->
                    //        <tr style='background:#f2f2f2;'>
                    //          <td style='padding:10px; font-weight:bold;'>Klient</td>
                    //          <td style='padding:10px;' colspan='3'>
                    //            <b>{rentalClient.FullName}</b><br/>
                    //            {rentalClient.Street} {rentalClient.Number}<br/>
                    //            {rentalClient.ZIPCode} {rentalClient.City}, {rentalClient.Country}<br/>
                    //            Telefon: {rentalClient.Phone}<br/>
                    //            Email: {rentalClient.Email}
                    //          </td>
                    //        </tr>

                    //        <!-- Kontakty do domu -->
                    //        {(houseThis?.Contacts != null && houseThis.Contacts.Any() ? string.Join("", houseThis.Contacts.Select(contact => $@"
                    //        <tr>
                    //          <td style='padding:10px; font-weight:bold;'>Kontakt do domu</td>
                    //          <td style='padding:10px;' colspan='3'>
                    //            <b>{contact.Name}</b><br/>
                    //            {(contact.Addresses != null ? string.Join("<br/>", contact.Addresses.Select(addr => $"{addr.Street}<br/>{addr.PostalCode} {addr.City}, {addr.Country}")) : "")}
                    //            {(contact.PhoneNumbers != null ? string.Join("<br/>", contact.PhoneNumbers.Select(p => $"Telefon: {p.Number}")) : "")}
                    //            {(contact.EmailAddresses != null ? string.Join("<br/>", contact.EmailAddresses.Select(e => $"Email: {e.Email}")) : "")}
                    //          </td>
                    //        </tr>
                    //        ")) : "")}

                    //        <!-- Regulamin -->
                    //        <tr>
                    //          <td style='padding:10px; font-weight:bold;'>Regulamin</td>
                    //          <td style='padding:10px;' colspan='3'>{houseThis?.RentalRules}</td>
                    //        </tr>

                    //      </tbody>
                    //    </table>

                    //    </body>
                    //    </html>
                    //    ";

                    // Obliczenia
                    decimal zadatek = rentalHouse.ToPay * 0.3m;
                    DateTime zadatekTermin = DateTime.Now.AddDays(7);
                    DateTime pelnaKwotaTermin = rentalHouse.From.AddDays(-30);

                               string mailBody = $@"
                                <html>
                                <head>
                                </head>
                                <body style='font-family: Arial, sans-serif; font-size:14px; color:#000000;'>

                                <h2 style='color:#000000;'>Nowa rezerwacja domu</h2>

                                <table style='width:100%; border-collapse:collapse; border:2px solid #000; font-size:16px; color:#000;margin-bottom: 30px;'>
                                  <tbody>
                                    <!-- Informacje o domu -->
                                    <tr style='background:#f2f2f2;'>
                                      <td style='padding:10px; font-weight:bold; width:200px;'>Dom</td>
                                      <td style='padding:10px;' colspan='2'>{houseThis?.Name}</td>
                                    </tr>

                                    <!-- Termin rezerwacji -->
                                    <tr>
                                      <td style='padding:10px; font-weight:bold;'>Termin</td>
                                      <td style='padding:10px;' colspan='2'>{rentalHouse.From:dd.MM.yyyy} - {rentalHouse.To:dd.MM.yyyy} <br/>({rentalHouse.HowManyDaysFromSelect} dni)</td>
                                    </tr>

                                    <!-- Kwoty -->
                                    <tr style='background:#f2f2f2; text-align:center;'>
                                      <td style='padding:10px; font-weight:bold;'>Do zapłaty (łącznie)</td>
                                      <td style='padding:10px; font-weight:bold;'>Zadatek (30%)</td>
                                      <td style='padding:10px; font-weight:bold;'>Pozostała kwota</td>
                                    </tr>
                                    <tr style='text-align:center;'>
                                      <td style='padding:10px; font-weight:bold;'>{rentalHouse.ToPay.ToString("N2", CultureInfo.InvariantCulture)} €</td>
                                      <td style='padding:10px; font-weight:bold;'>
                                        {zadatek.ToString("N2", CultureInfo.InvariantCulture)} €<br/>
                                        <span style='font-weight:normal;'>Płatność do:</span><br/>
                                        {zadatekTermin:dd.MM.yyyy}
                                      </td>
                                      <td style='padding:10px; font-weight:bold;'>
                                        {(rentalHouse.ToPay - zadatek).ToString("N2", CultureInfo.InvariantCulture)} €<br/>
                                        <span style='font-weight:normal;'>Płatność do:</span><br/>
                                        {pelnaKwotaTermin:dd.MM.yyyy}
                                      </td>
                                    </tr>

                                    <!-- Informacje o kliencie -->
                                    <tr style='background:#f2f2f2;'>
                                      <td style='padding:10px; font-weight:bold;'>Klient</td>
                                      <td style='padding:10px;' colspan='2'>
                                        <b>{rentalClient.FullName}</b><br/>
                                        {rentalClient.Street} {rentalClient.Number}<br/>
                                        {rentalClient.ZIPCode} {rentalClient.City}, {rentalClient.Country}<br/>
                                        Telefon: {rentalClient.Phone}<br/>
                                        Email: {rentalClient.Email}
                                      </td>
                                    </tr>

                                    <!-- Kontakty do domu 
                                    {(houseThis?.Contacts != null && houseThis.Contacts.Any() ? string.Join("", houseThis.Contacts.Select(contact => $@"
                                    <tr>
                                      <td style='padding:10px; font-weight:bold;'></td>
                                      <td style='padding:10px;' colspan='2'>
                                        <b>{contact.Name}</b><br/>
                                        {(contact.Addresses != null ? string.Join("<br/>", contact.Addresses.Select(addr => $"{addr.Street}<br/>{addr.PostalCode} {addr.City}, {addr.Country}")) : "")}
                                        {(contact.PhoneNumbers != null ? string.Join("<br/>", contact.PhoneNumbers.Select(p => $"Telefon: {p.Number}")) : "")}
                                        {(contact.EmailAddresses != null ? string.Join("<br/>", contact.EmailAddresses.Select(e => $"Email: {e.Email}")) : "")}
                                      </td>
                                    </tr>
                                    ")) : "")}
                                    --> 

                                    <tr>
                                      <td style='padding:10px; font-weight:bold;'>Data:</td>
                                      <td style='padding:10px;' colspan='2'>{rentalHouse.CreationDate.ToString("dd.MM.yyyy HH:mm")}</td>
                                    </tr>

                                    <!-- Regulamin 
                                    <tr>
                                      <td style='padding:10px; font-weight:bold;'>Regulamin</td>
                                      <td style='padding:10px;' colspan='2'>{houseThis?.RentalRules}</td>
                                    </tr>
                                    -->

                                  </tbody>
                                </table>

                                <!-- Kontakty do domu --> 
                                {(houseThis?.Contacts != null && houseThis.Contacts.Any() ? string.Join("", houseThis.Contacts.Select(contact => $@"
                                    <b>{contact.Name}</b><br/>
                                    {(contact.Addresses != null ? string.Join("<br/>", contact.Addresses.Select(addr => $"{addr.Street}<br/>{addr.PostalCode} {addr.City}, {addr.Country}")) : "")}<br/>
                                    {(contact.PhoneNumbers != null ? string.Join("<br/>", contact.PhoneNumbers.Select(p => $"Telefon: {p.Number}")) : "")}<br/>
                                    {(contact.EmailAddresses != null ? string.Join("<br/>", contact.EmailAddresses.Select(e => $"Email: {e.Email}")) : "")}<br/>
                                    
                                ")) : "")}

                                </body>
                                </html>

                                ";


                    string test1 = mailBody;


                    await _emailService.SendEmailAsync("krzysztofranczyk@gmail.com", "Nowa rezerwacja domu", mailBody);

                    //// Pobranie regulaminu z bazy
                    //string regulaminText = await _regulaminRepository.GetRegulaminAsync();
                    //byte[] regulaminBytes = System.Text.Encoding.UTF8.GetBytes(regulaminText);

                    //// Przygotowanie załącznika
                    //var attachments = new Dictionary<string, (byte[] Content, string MimeType)>
                    //{
                    //    { "regulamin.txt", (regulaminBytes, "text/plain") }
                    //};

                    //// Wysyłka maila
                    //await _emailService.SendEmailAsync("krzysztofranczyk@gmail.com","Nowa rezerwacja domu",mailBody,attachments);


                    return RedirectToAction("ThanksForTheReservation", "GetCalendar", rentalHouse);

                }
                else 
                {
                    return RedirectToAction("Index");   
                }

            }
            return View(rentalClient);
        }


        public IActionResult DetailsInfo(DateTime start, DateTime end)
        {
            // Tutaj możesz pobrać szczegóły rezerwacji i przekazać do widoku
            ViewData["Start"] = start;
            ViewData["End"] = end;
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> CreateNewReservation([FromBody] ReservationRequest request)
        {
            var from = request.From.Date;
            var to = request.To.Date;

            if (await HasCollision(from, to))
                return Conflict("Termin już zajęty");

            // zapis lub sesja tymczasowa
            // ...

            return Ok(new
            {
                success = true,
                redirectUrl = "/Reservation/Details"
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


        public class ReservationDto
        {
            public string From { get; set; }
            public string To { get; set; }
        }

    }
}
