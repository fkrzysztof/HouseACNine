using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Data.HouseRentalData
{
    public class RentalClient
    {
        [Key]
        public int RentalClientId { get; set; }

        [Required(ErrorMessage = "Pole jest wymagane")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Pole jest wymagane")]
        public required string LastName { get; set; }

        [NotMapped]
        [Display(Name = "Klient")]
        public string FullName => $"{Name} {LastName}";

        //Kontakt

        [Required(ErrorMessage = "Pole jest wymagane")]
        [DataType(DataType.EmailAddress)]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Pole jest wymagane")]
        [DataType(DataType.PhoneNumber)]
        public required string Phone { get; set; }

        //Adres

        [Required(ErrorMessage = "Pole jest wymagane")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Nazwa powinna zawierac od 3 do 20 znaków")]
        [Display(Name = "Kraj")]
        public required string Country { get; set; }

        [Required(ErrorMessage = "Pole jest wymagane")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Nazwa powinna zawierac od 3 do 20 znaków")]
        [Display(Name = "Miasto")]
        public required string City { get; set; }

        [Required(ErrorMessage = "Pole jest wymagane")]
        [Display(Name = "Ulica")]
        public required string Street { get; set; }

        [Required(ErrorMessage = "Pole jest wymagane")]
        [Display(Name = "Numer")]
        public required string Number { get; set; }

        [Required(ErrorMessage = "Pole jest wymagane")]
        [Display(Name = "Kod")]
        public required string ZIPCode { get; set; }

        public ICollection<RentalHouse>? RentalHouses { get; set; }
    }
}
