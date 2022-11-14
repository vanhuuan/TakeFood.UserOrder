namespace TakeFood.UserOrder.ViewModel.Dtos.Order;

public class OrderPagingResponse
{
    public int Total { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public List<OrderAdminCard> Cards { get; set; }
}

public class OrderAdminCard
{
    public int Id { get; set; }
    public int Stt { get; set; }
    public string OrderId { get; set; }
    public string Address { get; set; }
    public string PhoneNumber { get; set; }
    public double Total { get; set; }
    public DateTime OrderDate { get; set; }
    public string State { get; set; }
}
