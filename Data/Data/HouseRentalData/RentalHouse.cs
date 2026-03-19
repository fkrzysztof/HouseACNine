using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Data.HouseRentalData
{
    [Index(nameof(ReservationNumber), IsUnique = true)]
    public class RentalHouse
    {
        [Key]
        public int RentalHouseID { get; set; }

        [Required]
        [MaxLength(20)]
        public string ReservationNumber { get; set; } = null!;

        [Display(Name = "Dom")]
        public int? HouseId { get; set; }

        [ForeignKey("HouseId")]
        public House? House { get; set; }

        [Required]
        [NotMapped]
        public int HowManyDaysFromSelect 
        {
            get
            {
                if (To.Date <= From.Date)
                    return 0;

                return (To.Date - From.Date).Days + 1;
            }
        }


        // ---------------- PŁATNOŚCI ----------------


        [Column(TypeName = "money")]
        [Display(Name = "Cena wynajmu")]
        public decimal ToPay { get; set; }

        [Column(TypeName = "money")]
        [Display(Name = "Zaliczka")]
        public decimal DepositAmount { get; set; }

        [Display(Name = "Termin zaliczki")]
        [DataType(DataType.Date)]
        public DateTime? DepositDueDate { get; set; }

        [Column(TypeName = "money")]
        [Display(Name = "Pozostała kwota")]
        public decimal RemainingAmount { get; set; }

        [Display(Name = "Termin płatności całości")]
        [DataType(DataType.Date)]
        public DateTime? RemainingDueDate { get; set; }

        [Display(Name = "Data zapłaty zaliczki")]
        public DateTime? DepositPaidDate { get; set; }

        [Display(Name = "Data zapłaty całości")]
        public DateTime? RemainingPaidDate { get; set; }

        [Display(Name = "Przypomnienie o zaliczce wysłane")]
        public bool DepositReminderSent { get; set; }

        [Display(Name = "Przypomnienie o płatności całości wysłane")]
        public bool RemainingReminderSent { get; set; }



        //[Display(Name = "Klient")]
        //[ForeignKey("RentalClientId")]
        //public RentalClient? RentalClient { get; set; }

        [Display(Name = "Klient")]
        public int? RentalClientId { get; set; }

        [ForeignKey("RentalClientId")]
        public RentalClient? RentalClient { get; set; }


        [DataType(DataType.Date)]
        [Display(Name = "Od")]
        public DateTime From { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Do")]
        public DateTime To { get; set; }

        [Display(Name = "Status")]
        public int? RentalStatusID { get; set; }
        [ForeignKey("RentalStatusID")]
        public RentalStatus? RentalStatus { get; set; }

        [Display(Name = "Data utworzenia")]
        public DateTime CreationDate { get; set; }

        [Display(Name = "Adnotacje")]
        public string? Annotations { get; set; }

        public bool IsActive { get; set; }



    }
}
