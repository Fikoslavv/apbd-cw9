namespace apbd_cw9;

public class Order
{
    public int IdOrder { get; set; } = -1;
    public int IdProduct { get; set; }
    public int Amount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? FulfilledAt { get; set; }
}
