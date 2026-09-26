using StorageSystem.Domain.Services;

namespace StorageSystem.Domain.Entities;

public class Brand
{
    public int Id { get; private set; }
    public string Name { get; private set; } = default!;
    public ICollection<Product> Products { get; private set; } = new List<Product>();

    private Brand() { }

    public static Brand Create(string name)
    {
        GuardClause.AgainstNullOrEmptyName(name);

        return new Brand
        {
            Name = name.Trim()
        };
    }

    public void Rename(string newName)
    {
        GuardClause.AgainstNullOrEmptyName(newName);

        Name = newName.Trim();
    }

}