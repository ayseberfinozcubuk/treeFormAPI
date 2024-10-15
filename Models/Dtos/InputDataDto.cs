public class InputDataDto
{
    public string Name { get; set; }
    public List<StoreDto> Stores { get; set; }
}

public class StoreDto
{
    public string StoreName { get; set; }
    public List<ItemDto> Items { get; set; }
}

public class ItemDto
{
    public string ItemName { get; set; }
}
