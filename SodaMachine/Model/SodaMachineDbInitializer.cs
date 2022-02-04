using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace SodaMachine
{

    public interface ISodaMachineDbInitializer
    {
        void Initialize();
        void SeedData();
    }

    public class SodaMachineDbInitializer : ISodaMachineDbInitializer
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SodaMachineDbInitializer> _logger;

        public SodaMachineDbInitializer(
            IServiceScopeFactory scopeFactory,
            ILogger<SodaMachineDbInitializer> logger
            )
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Initialize()
        {
            _logger.LogDebug("Initialize() at the start");
            using (var serviceScope = _scopeFactory.CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<SodaMachineDbContext>())
                {
                    context.Database.EnsureCreated();
                }
            }
            _logger.LogDebug("Initialize() at the end");
        }

        public void SeedData()
        {
            _logger.LogDebug("SeedData() at the start");
            AddSodaInventoryDefaultData();
            _logger.LogDebug("SeedData() at the end");
        }

        private void AddSodaInventoryDefaultData()
        {
            _logger.LogDebug("AddSodaInventory() at the start");
            int inventoryCount = 0;
            using (var serviceScope = _scopeFactory.CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<SodaMachineDbContext>())
                {
                    var id = Guid.NewGuid();
                    var coke = new Soda(id, NameOfSoda.Coke, 50, 5);
                    context.Add<Soda>(coke);
                    id = Guid.NewGuid();
                    var fanta = new Soda(id, NameOfSoda.Fanta, 32, 4);
                    context.Add<Soda>(fanta);
                    id = Guid.NewGuid();
                    var sprite = new Soda(id, NameOfSoda.Sprite, 15, 3);
                    context.Add<Soda>(sprite);
                    context.SaveChanges();

                    inventoryCount = context.Inventory.Count();
                }
            }
            _logger.LogDebug($"AddSodaInventory() inventoryCount {inventoryCount} at the end");
        }


        //end of class
    }
}
