using Microsoft.EntityFrameworkCore;

namespace SodaMachine
{
    public class SodaMachineDbContext : DbContext
    {
        public SodaMachineDbContext(DbContextOptions<SodaMachineDbContext> options) : base(options)
        {
        }

        public DbSet<Soda> Inventory { get; set; }
        public DbSet<Customer> Customers { get; set; }
        // end of class
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

       

        //end of class
    }
}
