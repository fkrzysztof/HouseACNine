using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Data.HouseRentalData
{
    public class Address
    {
        [Key]
        public int AddressId { get; set; }

        [Required(ErrorMessage = "Street is required")]
        [StringLength(100, ErrorMessage = "Street name is too long")]
        public required string Street { get; set; }

        [Required(ErrorMessage = "City is required")]
        [StringLength(50, ErrorMessage = "City name is too long")]
        public required string City { get; set; }

        [Required(ErrorMessage = "Postal code is required")]
        [RegularExpression(@"^\d{2}-\d{3}$", ErrorMessage = "Postal code must be in format 00-000")]
        public required string PostalCode { get; set; }

        [Required(ErrorMessage = "Country is required")]
        [StringLength(50, ErrorMessage = "Country name is too long")]
        public required string Country { get; set; }

        public int ContactId { get; set; }

        [ForeignKey("ContactId")]
        public Contact? Contact { get; set; }

    }
}
