using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Data.HouseRentalData
{
    //nazwa + grafika
    public class GeneralInformation
    {
        [Key]
        public int GeneralInformationId { get; set; }
        
        [Display(Name = "Nazwa")]
        public required string Name { get; set; }

        //IMG
        [NotMapped]
        public IFormFile? FormFileItem { get; set; }
        public MyFile? Image { get; set; }

        [ForeignKey("HouseId")]
        public House? House { get; set; }
    }
}
