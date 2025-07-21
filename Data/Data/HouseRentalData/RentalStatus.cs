using System.ComponentModel.DataAnnotations;

namespace Data.Data.HouseRentalData
{
    public class RentalStatus
    {
        [Key]
        public int RentalStatusID { get; set; }

        
        [Display(Name = "Status")]
        public String? Name { get; set; }

        public ICollection<RentalHouse>? RentalHouses { get; set; }

        public bool IsActive { get; set; }
    }
}
