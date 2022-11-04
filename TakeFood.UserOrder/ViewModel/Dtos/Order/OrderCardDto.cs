namespace TakeFood.UserOrder.ViewModel.Dtos.Order;

public class OrderCardDto
{
    public string OrderId { get; set; }
    public string StoreName { get; set; }
    public string State { get; set; }
    public double Total { get; set; }
    public int FoodQuantity { get; set; }
}
