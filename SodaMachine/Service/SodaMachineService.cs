using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SodaMachine
{
    public interface ISodaMachineService
    {
        Task<Customer> AddCustomer(Guid id, double money = 100);
        Task<Customer> GetCustomer(Guid id);
        Task<Customer> AddSelectedItemToCustomerSelection(Guid id, Soda selectedItem);
        Task AdjustCustomerBalance(Guid id);
        Task<double> GetCurrentCustomerBalance(Guid id);
        Task AdjustInventory(Guid customerId, Soda selectedItem);
        Task<int> GetInventory(Soda selectedItem);
        Soda GetSelectedSoda(string name);
    }
    public sealed class SodaMachineService : ISodaMachineService
    {
        private readonly ILogger<SodaMachineService> _logger;
        private readonly SodaMachineDbContext _sodaMachineDbContext;
        public SodaMachineService(
            SodaMachineDbContext sodaMachineDbContext,
            ILogger<SodaMachineService> logger
            )
        {

            _sodaMachineDbContext = sodaMachineDbContext ?? throw new ArgumentNullException(nameof(sodaMachineDbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Customer> AddCustomer(Guid id, double money = 100)
        {
            _logger.LogDebug("AddCustomer at the start");
            var existingCustomer = await _sodaMachineDbContext.Customers
                                          .FirstOrDefaultAsync(q => q.Id == id);
            if (existingCustomer != null)
            {
                return existingCustomer;
            }
            var customer = new Customer(id, money);
            await _sodaMachineDbContext.AddAsync<Customer>(customer);
            await _sodaMachineDbContext.SaveChangesAsync();
            _logger.LogDebug("AddCustomer at the end");
            return customer;
        }

        public async Task<Customer> GetCustomer(Guid id)
        {
            var customer = await _sodaMachineDbContext.Customers
                                                      .Include(q => q.SelectedItems)
                                                      .FirstOrDefaultAsync(q => q.Id == id);
            if (customer == null)
            {
                throw new NotFoundException(id.ToString(), "customer");
            }
            return customer;
        }

        public async Task<Customer> AddSelectedItemToCustomerSelection(Guid id, Soda selectedItem)
        {
            var customer = await _sodaMachineDbContext.Customers
                                                      .FirstOrDefaultAsync(q => q.Id == id);
            if (customer == null)
            {
                throw new NotFoundException(id.ToString(), "customer");
            }
            _sodaMachineDbContext.Remove(customer);
            await _sodaMachineDbContext.SaveChangesAsync();

            var updatedCustomerEntity = new Customer(id, customer.Money);
            updatedCustomerEntity.AddToSelectedItems(selectedItem);
            _sodaMachineDbContext.Add(updatedCustomerEntity);
            await _sodaMachineDbContext.SaveChangesAsync();
            return updatedCustomerEntity;
        }

        public async Task AdjustCustomerBalance(Guid id)
        {
            var customer = await _sodaMachineDbContext.Customers
                                                      .Include(q => q.SelectedItems)
                                                      .FirstOrDefaultAsync(q => q.Id == id);
            if (customer == null)
            {
                throw new NotFoundException(id.ToString(), "customer");
            }
            if (!customer.SelectedItems.Any())
            {
                return;
            }

            foreach (var selectedItem in customer.SelectedItems)
            {
                customer.DetectMoney(selectedItem.Price);
            }
            _sodaMachineDbContext.Remove(customer);
            await _sodaMachineDbContext.SaveChangesAsync();

            var updatedCustomerEntity = new Customer(id, customer.Money);
            foreach (var selectedItem in customer.SelectedItems)
                updatedCustomerEntity.AddToSelectedItems(selectedItem);
            _sodaMachineDbContext.Add(updatedCustomerEntity);
            await _sodaMachineDbContext.SaveChangesAsync();
        }

        public async Task<double> GetCurrentCustomerBalance(Guid id)
        {
            var customer = await _sodaMachineDbContext.Customers
                                          .AsNoTracking()
                                          .FirstOrDefaultAsync(q => q.Id == id);
            if (customer == null)
            {
                _logger.LogTrace($"customer not found {id}");
                return 0;
            }

            return customer.Money;
        }

        public async Task AdjustInventory(Guid customerId, Soda selectedItem)
        {
            var customer = await _sodaMachineDbContext.Customers
                                                         .Include(q => q.SelectedItems)
                                                         .FirstOrDefaultAsync(q => q.Id == customerId);
            if (customer == null)
            {
                throw new NotFoundException(customerId.ToString(), "customer");
            }

            var selectedSoda = await _sodaMachineDbContext.Inventory
                                                          .FirstOrDefaultAsync(q => q.Name == selectedItem.Name);
            if (selectedSoda == null)
            {
                throw new NotFoundException(selectedSoda.Id.ToString(), selectedItem.Name);
            }

            foreach (var item in customer.SelectedItems)
            {
                selectedSoda.RemoveQuantity(1);
            }

            _sodaMachineDbContext.Remove(selectedSoda);
            await _sodaMachineDbContext.SaveChangesAsync();

            var updatedSodaEntity = new Soda(selectedSoda.Id, selectedSoda.Name, selectedSoda.Quantity, selectedSoda.Price);
            _sodaMachineDbContext.Add(updatedSodaEntity);
            await _sodaMachineDbContext.SaveChangesAsync();
        }

        public async Task<int> GetInventory(Soda selectedItem)
        {
            var selectedSoda = await _sodaMachineDbContext.Inventory
                                                          .FirstOrDefaultAsync(q => q.Name == selectedItem.Name);
            if (selectedSoda == null)
            {
                throw new NotFoundException(selectedSoda.Id.ToString(), selectedItem.Name);
            }
            return selectedSoda.Quantity;
        }

        public Soda GetSelectedSoda(string name)
        {
            var selectedSoda = _sodaMachineDbContext.Inventory
                                                          .FirstOrDefault(q => q.Name == name);
            if (selectedSoda == null)
            {
                throw new NotFoundException(selectedSoda.Id.ToString(), name);
            }

            return selectedSoda;
        }


        //end of class
    }
}
