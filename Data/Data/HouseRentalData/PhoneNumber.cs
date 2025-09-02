using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Data.HouseRentalData
{
    public class PhoneNumber
    {
        [Key] 
        public int PhoneNumberId { get; set; }

        [Phone(ErrorMessage = "Invalid phone number")]
        public required string Number { get; set; }

        public int ContactId { get; set; }

        [ForeignKey("ContactId")]
        public Contact? Contact { get; set; }
    }
}
