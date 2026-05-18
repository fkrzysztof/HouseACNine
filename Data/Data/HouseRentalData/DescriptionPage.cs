using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Enums;

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


        public int HouseId { get; set; }
        public House? House { get; set; }

        [Display(Name = "Kolejność")]
        public int DisplayOrder { get; set; }

        //IMG COL
        [NotMapped]
        public List<IFormFile>? FormFileItems { get; set; }
        public ICollection<MyFile> Images { get; set; } = new List<MyFile>();

        public SectionType EnabledSections { get; set; } = SectionType.None;

    }
}
