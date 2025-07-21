using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Data.HouseRentalData
{
    public class RentalHouse
    {
        [Key]
        public int RentalHouseID { get; set; }

        [Display(Name = "Dom")]
        public int? HouseId { get; set; }

        [ForeignKey("HouseId")]
        public House? House { get; set; }

        [Required]
        [NotMapped]
        public int HowManyDaysFromSelect { get; set; }

        [NotMapped]
        [Column(TypeName = "money")]
        [Display(Name = "Do zapłaty")]
        public decimal ToPay { get; set; }

        [Display(Name = "Klient")]
        [ForeignKey("RentalClientId")]
        public RentalClient? RentalClient { get; set; }

        //[Display(Name = "Klient")]
        //public string ApplicationUserID { get; set; }
        //[ForeignKey("Id")]
        //public ApplicationUser ApplicationUser { get; set; }

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
