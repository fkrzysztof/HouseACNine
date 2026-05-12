using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Data.HouseRentalData
{
    public class Contact
    {
        [Key]
        public int ContactId { get; set; }

        [Required(ErrorMessage = "Is required")]
        public required string Name { get; set; }

        public List<Address> Addresses { get; set; } = new();
        public List<EmailAddress> EmailAddresses { get; set; } = new();
        public List<PhoneNumber> PhoneNumbers { get; set; } = new();

        public int? HouseId { get; set; }

        [ForeignKey("HouseId")]
        public House? House { get; set; }

    }
}
