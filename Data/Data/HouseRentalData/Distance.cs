using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Data.HouseRentalData
{
    public class Distance
    {
        [Key]
        public int DistanceID { get; set; }

        [Required]
        public required string Name { get; set; }

        public ICollection<DistanceItem>? DistanceItems { get; set; }

        [ForeignKey("HouseId")]
        public House? House { get; set; }

        //Img
        [NotMapped]
        public IFormFile? FormFileItem { get; set; }
        public MyFile? Image { get; set; }
    }
}
