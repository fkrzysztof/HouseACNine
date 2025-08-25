using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Data.HouseRentalData
{
    public class MyFile
    {
        [Key]
        public int FileID { get; set; }
        public string? Path { get; set; }

        //links ************************

        public int? GeneralInformationId { get; set; }
        [ForeignKey("GeneralInformationId")]
        public GeneralInformation? GeneralInformation { get; set; }

        public int? DescriptionPageId { get; set; }
        [ForeignKey("DescriptionPageId")]
        public DescriptionPage? DescriptionPage { get; set; }

        public int? DetailedInformationId { get; set; }
        [ForeignKey("DetailedInformationId")]
        public DetailedInformation? DetailedInformation { get; set; }

        public int? DistanceID { get; set; }
        [ForeignKey("DistanceID")]
        public Distance? Distance { get; set; }
    }
}
        