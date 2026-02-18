using Happenings.Services.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Happenings.Services.Database
{
    public class HappeningsContextFactory
        : IDesignTimeDbContextFactory<HappeningsContext>
    {
        public HappeningsContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<HappeningsContext>();

            optionsBuilder.UseSqlServer(
                "Server=.;Database=HappeningsDB;Trusted_Connection=True;TrustServerCertificate=True");

            return new HappeningsContext(optionsBuilder.Options);
        }
    }
}
