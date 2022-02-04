using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SodaMachine.Tests
{
    public class SodaMachineServiceTests
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

            _serviceProvider = new ServiceCollection()
             .AddSingleton<IConfiguration>(configuration)
             .AddSingleton(Mock.Of<ILogger<SodaMachineService>>())
             .AddTransient<ISodaMachineService, SodaMachineService>()
             .AddDbContext<SodaMachineDbContext>(options => options.UseInMemoryDatabase("SodaMachine"))
             .BuildServiceProvider();
        }

        [Test]
        public void Test_SodaMachineService_ReturnsTrueIfSodaMachineServiceNotNull()
        {
            var sodaMachineService = _serviceProvider.GetService<ISodaMachineService>();
            Assert.IsNotNull(sodaMachineService);
        }

        [Test]
        public async Task Test_AddCustomer_ShouldAddCustomerToStorage()
        {
            var sodaMachineService = _serviceProvider.GetService<ISodaMachineService>();
            var id = Guid.NewGuid();
            await sodaMachineService.AddCustomer(id, 40);
            var customer = await sodaMachineService.GetCustomer(id);
            Assert.IsNotNull(customer);
        }

        [Test]
        public async Task Test_AddSelectedItemToCustomerSelection_ShouldAddSelectedItemToCustomerSelection()
        {
            var sodaMachineService = _serviceProvider.GetService<ISodaMachineService>();
            var id = Guid.NewGuid();
            await sodaMachineService.AddCustomer(id, 40);
            var customer = await sodaMachineService.GetCustomer(id);
            var sodaId = Guid.NewGuid();
            var selectedItem = new Soda(sodaId, NameOfSoda.Coke, 5, 30);
            await sodaMachineService.AddSelectedItemToCustomerSelection(id, selectedItem);
            customer = await sodaMachineService.GetCustomer(id);
            Assert.IsTrue(customer.SelectedItems.Count > 0);
        }

        [Test]
        public async Task Test_AdjustCustomerBalance_ShouldAdjustCustomerBalance()
        {
            var sodaMachineService = _serviceProvider.GetService<ISodaMachineService>();
            var id = Guid.NewGuid();
            await sodaMachineService.AddCustomer(id, 40);
            var sodaId = Guid.NewGuid();
            var selectedItem = new Soda(sodaId, NameOfSoda.Coke, 5, 30);
            await sodaMachineService.AddSelectedItemToCustomerSelection(id, selectedItem);
            await sodaMachineService.AdjustCustomerBalance(id);
            var currentCustomerBalance = await sodaMachineService.GetCurrentCustomerBalance(id);
            Assert.IsTrue(currentCustomerBalance == 10);
        }


        [Test]
        public async Task Test_AdjustInventory_ShouldAdjustInventory()
        {
            var sodaMachineService = _serviceProvider.GetService<ISodaMachineService>();
            var id = Guid.NewGuid();
            await sodaMachineService.AddCustomer(id, 40);
            var sodaId = Guid.NewGuid();
            var selectedItem = new Soda(sodaId, NameOfSoda.Coke, 5, 30);
            await sodaMachineService.AddSelectedItemToCustomerSelection(id, selectedItem);
            await sodaMachineService.AdjustCustomerBalance(id);
            await sodaMachineService.AdjustInventory(id, selectedItem);
            var inventory = await sodaMachineService.GetInventory(selectedItem);
            Assert.IsTrue(inventory == 4);
        }


        // end of class
    }
}
