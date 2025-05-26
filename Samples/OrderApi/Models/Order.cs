public class Order
{
    public Guid Id { get; set; }
    public string Product { get; set; } = default!;
    public int Quantity { get; set; }
    public DateTime CreatedAt { get; set; }
}
