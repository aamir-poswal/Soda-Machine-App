using Ardalis.GuardClauses;
using System;

namespace SodaMachine
{
    public sealed class Soda
    {
        public Soda(
            Guid id,
            string name,
            int quantity,
            int price
            )
        {
            Id = id;
            Name = Guard.Against.NullOrWhiteSpace(name, nameof(name));
            Quantity = Guard.Against.NegativeOrZero(quantity, nameof(quantity));
            Price = Guard.Against.NegativeOrZero(price, nameof(price));
        }
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public int Quantity { get; private set; }
        public int Price { get; private set; }
        public void AddQuantity(int quantityToAdd)
        {
            if (quantityToAdd == default(int))
            {
                throw new ArgumentNullException(nameof(quantityToAdd));
            }

            Quantity = Quantity + quantityToAdd;
        }
        public void RemoveQuantity(int quantityToRemove)
        {
            if (quantityToRemove == default(int))
            {
                throw new ArgumentNullException(nameof(quantityToRemove));
            }
            if (Quantity <= 0)
            {
                throw new Exception("There is not enough inventory available");
            }
            Quantity = Quantity - quantityToRemove;
        }

        // end of class
    }
}
