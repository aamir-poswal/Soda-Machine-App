using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SodaMachine.Tests
{
    public class ModelTests
    {
        private ServiceProvider _serviceProvider;

        [SetUp]
        public void Setup()
        {
            var builder = new ConfigurationBuilder()
                          .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                          .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                          .AddEnvironmentVariables();

            var configuration = builder.Build();

            var options = new DbContextOptionsBuilder<SodaMachineDbContext>()
                        .UseInMemoryDatabase(databaseName: "SodaMachine")
                        .Options;


            _serviceProvider = new ServiceCollection()
             .AddSingleton<IConfiguration>(configuration)
             .AddSingleton(x => new SodaMachineDbContext(options))
             //.AddDbContext<SodaMachineDbContext>(options => options.UseInMemoryDatabase("SodaMachine"))
             .BuildServiceProvider();

        }

        [Test]
        public void Test_SodaMachineDbContextConnection_ReturnsTrueIfCanConnectToSodaMachine()
        {
            var sodaMachineDbContext = _serviceProvider.GetService<SodaMachineDbContext>();
            Assert.IsTrue(sodaMachineDbContext.Database.CanConnect());
        }

        [Test]
        public async Task Test_AddInventory_ShouldAddInventory()
        {
            var sodaMachineDbContext = _serviceProvider.GetService<SodaMachineDbContext>();
            var id = Guid.NewGuid();
            var inventory = new Soda(id, NameOfSoda.Coke, 5, 30);
            await sodaMachineDbContext.AddAsync<Soda>(inventory);
            await sodaMachineDbContext.SaveChangesAsync();
            var inventoryCount = await sodaMachineDbContext.Inventory.CountAsync();
            Assert.IsTrue(inventoryCount == 1);
        }

        [Test]
        public async Task Test_AddCustomer_ShouldAddCustomer()
        {
            var sodaMachineDbContext = _serviceProvider.GetService<SodaMachineDbContext>();
            var customer = new Customer(Guid.NewGuid(), 30);
            await sodaMachineDbContext.AddAsync<Customer>(customer);
            await sodaMachineDbContext.SaveChangesAsync();
            var customerCount = await sodaMachineDbContext.Customers.CountAsync();
            Assert.IsTrue(customerCount == 1);
        }

        [Test]
        public async Task Test_AddSelectedItem_ShouldAddSelectedItemForCustomerToStorage()
        {


            var sodaMachineDbContext = _serviceProvider.GetService<SodaMachineDbContext>();
            var sodaId = Guid.NewGuid();
            var selectedItem = new Soda(sodaId, NameOfSoda.Coke, 5, 30);
            var id = Guid.NewGuid();
            var customer = new Customer(id, 30);
            customer.AddToSelectedItems(selectedItem);
            await sodaMachineDbContext.AddAsync<Customer>(customer);
            await sodaMachineDbContext.SaveChangesAsync();

            var selectedItemByCustomerCount = await sodaMachineDbContext.Customers
                                                                        .Include(q => q.SelectedItems)
                                                                        .FirstOrDefaultAsync();

            Assert.IsTrue(selectedItemByCustomerCount.SelectedItems.Count > 0);
        }

        [Test]
        public async Task Test_UpdateSelectedItem_ShouldUpdateSelectedItemForCustomerToStorage()
        {


            var sodaMachineDbContext = _serviceProvider.GetService<SodaMachineDbContext>();
            var sodaId = Guid.NewGuid();
            var selectedItem = new Soda(sodaId, NameOfSoda.Coke, 5, 30);
            var id = Guid.NewGuid();
            var customer = new Customer(id, 30);
            await sodaMachineDbContext.AddAsync<Customer>(customer);
            await sodaMachineDbContext.SaveChangesAsync();
            var returnedCustomer = await sodaMachineDbContext.Customers.FirstOrDefaultAsync(q => q.Id == id);
            sodaMachineDbContext.Remove(returnedCustomer);
            await sodaMachineDbContext.SaveChangesAsync();
            var newCustomerEntity = new Customer(id, returnedCustomer.Money);
            newCustomerEntity.AddToSelectedItems(selectedItem);
            await sodaMachineDbContext.AddAsync<Customer>(newCustomerEntity);
            await sodaMachineDbContext.SaveChangesAsync();
            await sodaMachineDbContext.SaveChangesAsync();
            var selectedItemByCustomerCount = await sodaMachineDbContext.Customers
                                                                        .Include(q => q.SelectedItems)
                                                                        .FirstOrDefaultAsync();

            Assert.IsTrue(selectedItemByCustomerCount.SelectedItems.Count > 0);
        }



        // end of class
    }
}
