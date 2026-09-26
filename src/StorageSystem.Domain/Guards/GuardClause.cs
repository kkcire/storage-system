using StorageSystem.Domain.Exceptions;

namespace StorageSystem.Domain.Guards;

public static class GuardClause
{
    public static void AgainstZeroOrNegativeId(int id)
    {
        if (id <= 0)
            throw new DomainException("The ID cannot be zero or negative.");
    }

    public static void AgainstNegativePrice(decimal price)
    {
        if (price < 0)
            throw new DomainException("The price cannot be negative.");
    }

    public static void AgainstNegativeQuantity(int quantity)
    {
        if (quantity < 0)
            throw new DomainException("The quantity cannot be negative.");
    }

    public static void AgainstNullOrWhiteSpaceName(string name)
    {

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("The name cannot be null or empty.");
    }
}