using TakeFood.UserOrder.ViewModel.Dtos;
using TakeFood.UserOrder.ViewModel.Dtos.Order;

namespace TakeFood.UserOrder.Service;

public interface IOrderService
{
    Task CreateOrderAsync(CreateOrderDto dto, string userId);
    Task CancelOrderAsync(String orderId, string userId);
    Task<List<OrderCardDto>> GetUserOrders(string userId, int index);
    Task<NotifyDto> GetNotifyInfo(string storeId);
    Task<OrderDetailDto> GetOrderDetail(string userId, string orderId);
}
