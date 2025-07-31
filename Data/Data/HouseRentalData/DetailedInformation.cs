using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Data.HouseRentalData
{
    //nazwa i grafika
    public class DetailedInformation
    {
        [Key] 
        public int DetailedInformationId { get; set; }

        public required string Name  { get; set; }

        public ICollection<DetailedInformationItem>? DetailedInformationItems { get; set; }


        [ForeignKey("HouseId")]
        public House? House { get; set; }

        //Img
        [NotMapped]
        public IFormFile? FormFileItem { get; set; }
        public MyFile? Image { get; set; }

    }
}
