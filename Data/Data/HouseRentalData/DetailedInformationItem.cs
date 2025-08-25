using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Data.HouseRentalData
{
    public class DetailedInformationItem
    {
        [Key]
        public int DetailedInformationItemId { get; set; }

        [Display( Name = "Opis")]
        public required string Description { get; set; }

        [ForeignKey("DetailedInformationId")]
        public DetailedInformation? DetailedInformation { get; set; }

    }
}
