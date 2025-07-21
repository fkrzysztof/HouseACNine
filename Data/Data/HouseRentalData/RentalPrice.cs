using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Data.HouseRentalData
{
    public class RentalPrice
    {
        [Key]
        public int RentalPriceID { get; set; }

        public int? HouseId { get; set; }

        [ForeignKey("HouseId")]
        public House? House { get; set; }

        [Required]
        [Column(TypeName = "money")]
        [Display(Name = "Cena / 7+ dni")]
        public decimal OneWeek { get; set; }

        [Required]
        [Column(TypeName = "money")]
        [Display(Name = "Cena / 14+ dni")]
        public decimal TwoWeeks { get; set; }

        [Display(Name = "Obowiązuje od")]
        public DateTime? DateTimeFrom { get; set; }

        [Display(Name = "Obowiązuje do")]
        public DateTime? DateTimeTo { get; set; }

        public bool IsActive { get; set; }
    }
}
