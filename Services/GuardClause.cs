using StorageSystem.Data;
using StorageSystem.Entities;

namespace StorageSystem.Services;

public static class GuardClause
{
    public static void ValidateZeroOrNegativeId(int id)
    {
        if (id <= 0)
            throw new ArgumentOutOfRangeException(nameof(id), "The ID cannot be zero or negative.");
    }

    public static void ValidateZeroOrNegativePrice(decimal price)
    {
        if (price <= 0)
            throw new ArgumentOutOfRangeException(nameof(price), "The price cannot be zero or negative.");
    }

    public static void ValidateNegativeQuantity(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "The quantity cannot be negative.");
    }

    public static void ValidateNullOrEmptyName(string name)
    {

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("The name cannot be null or empty.", nameof(name));
    }
}