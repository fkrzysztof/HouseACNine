using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Data.HouseRentalData
{
    public class DistanceItem
    {
        [Key]
        public int DistanceItemId { get; set; }

        [Required]
        [Display(Name = "Opis")]
        public required string NameInfo { get; set; }

        [Required]
        [Display(Name = "Odległość")]
        public required string DistanceInfo { get; set; }


        [ForeignKey("DistanceID")]
        public Distance? Distance {  get; set; }       
    }
}
