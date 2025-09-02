using System.ComponentModel.DataAnnotations;

namespace Data.Data.HouseRentalData
{
    public class Contact
    {
        [Key]
        public int ContactId { get; set; }

        [Required(ErrorMessage = "Is required")]
        public required string Name { get; set; }

        public ICollection<Address>? Addresses { get; set; }
        public ICollection<PhoneNumber>? PhoneNumbers { get; set; }
        public ICollection<EmailAddress>? EmailAddresses { get; set; }

    }
}
