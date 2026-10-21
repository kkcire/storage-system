using StorageSystem.Domain.Guards;

namespace StorageSystem.Domain.Entities;

public class Brand
{
    public int Id { get; private set; }
    public string Name { get; private set; } = default!;
    public ICollection<Product> Products { get; private set; } = new List<Product>();

    public static readonly int MaxNameLength = 50;

    private Brand() { } // EF Core

    public static Brand Create(string name)
    {
        GuardClause.AgainstNullOrWhiteSpaceName(name);
        GuardClause.AgainstMaxLengthName(name, MaxNameLength);

        return new Brand
        {
            Name = name.Trim()
        };
    }

    public void Rename(string newName)
    {
        GuardClause.AgainstNullOrWhiteSpaceName(newName);
        GuardClause.AgainstMaxLengthName(newName, MaxNameLength);

        Name = newName.Trim();
    }

}