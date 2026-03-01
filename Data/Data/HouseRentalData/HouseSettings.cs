using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Data.HouseRentalData
{

    public class HouseSettings
    {
        [Key]
        public int Id { get; set; }

        // --- Płatności ---
        [Display(Name = "Procent zaliczki")]
        [Precision(5, 2)] // EF Core 6+ automatycznie ustawia typ decimal(5,2)
        public decimal DepositPercentage { get; set; } = 30;

        [Display(Name = "Dni na zapłatę zaliczki")]
        public int DepositDueDays { get; set; } = 3;

        [Display(Name = "Dni przed przyjazdem na pełną płatność")]       //DepositPercentage, DepositDueDays,  FullPaymentDueDaysBeforeArrival , BankAccountIban, BankAccountSwift, BankAccountName, Currency, IsDefault
        public int FullPaymentDueDaysBeforeArrival { get; set; } = 7;

        // --- Bank ---
        [Display(Name = "Numer konta IBAN")]
        [MaxLength(34)]
        public string? BankAccountIban { get; set; }

        [Display(Name = "Kod SWIFT banku")]
        [MaxLength(11)]
        public string? BankAccountSwift { get; set; }

        [Display(Name = "Właściciel konta")]
        public string? BankAccountName { get; set; }

        [Display(Name = "Nazwa banku")]
        public string? BankName { get; set; }

        // --- Branding ---


        // logo domu – przechowujemy tylko nazwę pliku w folderze "uploads"
        [Display(Name = "Logo")]
        public string? LogoFileName { get; set; }

        [Display(Name = "Waluta płatności")]
        public string? Currency { get; set; } = "PLN";

        // Lista domów korzystających z tych ustawień
        public ICollection<House> Houses { get; set; } = new List<House>();

        //domysne ustawienia
        [Display(Name = "Domyślne ustawienia dla wszystkich obiektów")]
        public bool IsDefault { get; set; } = false;
    }


}
