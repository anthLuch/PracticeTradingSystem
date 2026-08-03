using System;

namespace TradingSystem.Core.Models
{
    public sealed class Order
    {
        public long Id { get; }

        public Side Side { get; }

        public decimal Price { get; }

        public long OriginalQuantity { get; }

        public long Quantity { get; private set; }

        public long Sequence { get; internal set; }

        public bool IsFilled => Quantity == 0;

        public Order(long id, Side side, decimal price, long originalQuantity)
        {
            if (price <= 0)
            {
                throw new ArgumentException("Price must be positive.", nameof(price));
            }
            if (originalQuantity <= 0)
            {
                throw new ArgumentException("Quantity must be positive.", nameof(originalQuantity));
            }

            Id = id;
            Side = side;
            Price = price;
            OriginalQuantity = originalQuantity;
            Quantity = originalQuantity;
        }

        internal void Filled(long amount)
        {
            if (amount <= 0 || amount > Quantity)
            {
                throw new ArgumentException("Invalid fill amount.", nameof(amount));
            }

            Quantity -= amount;
        }

    }
}