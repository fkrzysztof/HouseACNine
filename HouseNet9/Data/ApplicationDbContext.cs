
using Data.Data.HouseRentalData;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HouseNet9.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { 
        }
        public DbSet<RentalHouse> RentalHouses { get; set; } = default!;
        public DbSet<House> Houses { get; set; } = default!;
        public DbSet<RentalStatus> RentalStatus { get; set; } = default!;
        public DbSet<RentalPrice> RentalPrices { get; set; } = default!;
        public DbSet<RentalClient> RentalClients { get; set; } = default!;
        public DbSet<MyFile> MyFiles { get; set; } = default!;
        public DbSet<GeneralInformation> GeneralInformation { get; set; } = default!;
        public DbSet<DetailedInformation> DetailedInformation { get; set; } = default!;
        public DbSet<DetailedInformationItem> DetailedInformationItems { get; set; } = default!;
        public DbSet<DescriptionPage> DescriptionPages { get; set; } = default!;
        public DbSet<Distance> Distances { get; set; } = default!;
        public DbSet<DistanceItem> DistanceItems { get; set; } = default!;

    
    }
}
