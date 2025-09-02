using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Data.HouseRentalData
{
    public class EmailAddress
    {
        [Key]
        public int EmailAddressId { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public required string Email { get; set; }

        public int ContactId { get; set; }

        [ForeignKey("ContactId")]
        public Contact? Contact { get; set; }
    }
}
