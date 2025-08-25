using System.ComponentModel.DataAnnotations;

namespace Data.Data.HouseRentalData
{
    public class House
    {
        [Key]
        public int HouseId { get; set; }
        
        [Display(Name = "Nazwa")]
        public string? Name { get; set; }

        //Rezerwacja
        public ICollection<RentalHouse>? RentalHouses {  get; set; }    
        
        public ICollection<RentalPrice>? RentalPrices { get; set; }

        //info teksty
        [Display(Name = "Opis")]
        public string? Description { get; set; }

        [Display(Name = "Położenie")]
        public string? Location { get; set; }

        //lista grafika + info

        [Display(Name = "Informacje Ogólne")] // kolekcja nazwa + grafika
        public ICollection<GeneralInformation>? GeneralInformation { get; set; }

        [Display(Name = "Informacje szczegółowe")] // kolekcja obiektów, każdy składa sie z nazwy i kolekcji
        public ICollection<DetailedInformation>? DetailedInformation { get; set; }

        [Display(Name = "Opisy do strony")] // kolekcja opisow do strony
        public ICollection<DescriptionPage>? DescriptionPages { get; set; }

        [Display(Name = "Odległości")] // kolekcja informacji o odleglosciach
        public ICollection<Distance>? Distances { get; set; }

        public bool IsActive { get; set; }
    }
}
