using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Data.HouseRentalData
{
    public class DescriptionPage
    {
        [Key]
        public int DescriptionPageId { get; set; }


        [Display(Name = "Tytuł")]
        public String? Title { get; set; }

        [Display(Name = "Opis")]
        public String? Description { get; set; }

        [ForeignKey("HouseId")]
        public House? House { get; set; }

        //IMG
        [NotMapped]
        public IFormFile? FormFileItem { get; set; }
        public MyFile? Image { get; set; }
    }
}
