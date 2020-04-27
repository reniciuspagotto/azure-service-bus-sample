using AzureBusSample.Entity;
using Microsoft.EntityFrameworkCore;

namespace AzureBusSample.Context
{
    public class DataContext : DbContext
    {
        public DbSet<Product> Products { get; set; }

        public DataContext(DbContextOptions<DataContext> options) : base(options) { }
    }
}
