using StorageSystem.Data;
using StorageSystem.Entities;

namespace StorageSystem.Services;

public static class GuardClause
{
    public static void AgainstZeroOrNegativeId(int id)
    {
        if (id <= 0)
            throw new ArgumentOutOfRangeException(nameof(id), "The ID cannot be zero or negative.");
    }

    public static void AgainstZeroOrNegativePrice(decimal price)
    {
        if (price <= 0)
            throw new ArgumentOutOfRangeException(nameof(price), "The price cannot be zero or negative.");
    }

    public static void AgainstNegativeQuantity(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "The quantity cannot be negative.");
    }

    public static void AgainstNullOrEmptyName(string name)
    {

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("The name cannot be null or empty.", nameof(name));
    }
}