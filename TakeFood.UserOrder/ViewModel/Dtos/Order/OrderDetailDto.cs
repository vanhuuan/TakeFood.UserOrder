namespace TakeFood.UserOrder.ViewModel.Dtos.Order;

public class OrderDetailDto
{
    public string OrderId { get; set; }
    public string Address { get; set; }
    public string UserName { get; set; }
    public string PhoneNumber { get; set; }
    public string PaymentMethod { get; set; }
    public string State { get; set; }
    public string Note { get; set; }
    public double Total { get; set; }
    public List<FoodDetailsItem> Foods { get; set; }
}

public class FoodDetailsItem
{
    public string FoodId { get; set; }
    public double Total { get; set; }
    public string FoodName { get; set; }
    public int Quantity { get; set; }
    public List<ToppingDetailsItem> Toppings { get; set; }
}

public class ToppingDetailsItem
{
    public string ToppingId { get; set; }
    public string ToppingName { get; set; }
    public double Total { get; set; }
    public int Quantity { get; set; }
}
