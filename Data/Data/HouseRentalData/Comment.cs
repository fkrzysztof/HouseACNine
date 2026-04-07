using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Data.HouseRentalData
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ReservationCode { get; set; }

        [Required]
        public string Email { get; set; }

        public int HouseId { get; set; }

        [Range(1, 4)]
        public int Rating { get; set; }

        [Required]
        [StringLength(1000)]
        public string Text { get; set; }

        public string AuthorName { get; set; }
        public DateTime StayFrom { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsApproved { get; set; }

        public string? AdminText { get; set; }

        public string CountryCode { get; set; } // np. "PL", "DE", "US"


    }
}
