public class Item
{
    public ItemSO staticData;

    private string id;
    public string Id { get => GetID(); }
    
    public Item(ItemSO staticData)
    {
        id = System.Guid.NewGuid().ToString();
        this.staticData = staticData;
    }

    private string GetID()
    {
        return id = string.IsNullOrWhiteSpace(id) ? System.Guid.NewGuid().ToString() : id;
    }
}

public enum ItemUsageType
{
    Consumable,
    Head,
    Shoulder,
    Chest,
    Wrist,
    Hand,
    Legs,
    Feet,
}