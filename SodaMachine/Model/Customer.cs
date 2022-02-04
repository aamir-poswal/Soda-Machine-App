using Ardalis.GuardClauses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SodaMachine
{
    public sealed class Customer
    {
        public Customer(Guid id, double money)
        {
            Id = Guard.Against.Default(id, nameof(id));
            Money = Guard.Against.Negative(money, nameof(money));
        }
        public Guid Id { get; private set; }
        public double Money { get; private set; }
        public List<Soda> SelectedItems { get; private set; } = new List<Soda>();

        public void AddMoney(double? amount)
        {
            if (amount == null)
            {
                throw new ArgumentNullException(nameof(amount));
            }
            this.Money = this.Money + amount.Value;
        }
        public void DetectMoney(double? amount)
        {
            if (amount == null)
            {
                throw new ArgumentNullException(nameof(amount));
            }
            if (this.Money <= 0)
            {
                throw new Exception("There is not enough money to detect");
            }
            this.Money = this.Money - amount.Value;
        }

        public void AddToSelectedItems(Soda selectedItem)
        {
            if (selectedItem == null)
            {
                throw new ArgumentNullException(nameof(selectedItem));
            }
            this.SelectedItems.Add(selectedItem);
        }
        public void RemoveFromSelectedItems(Soda selectedItem)
        {
            if (selectedItem == null)
            {
                throw new ArgumentNullException(nameof(selectedItem));
            }
            if (!SelectedItems.Any())
            {
                throw new Exception("There is no item in the selected list");
            }
            this.SelectedItems.Remove(selectedItem);
        }

        // end of class
    }
}
