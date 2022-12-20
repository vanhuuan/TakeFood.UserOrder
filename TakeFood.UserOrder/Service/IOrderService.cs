using TakeFood.UserOrder.ViewModel.Dtos;
using TakeFood.UserOrder.ViewModel.Dtos.Order;

namespace TakeFood.UserOrder.Service;

public interface IOrderService
{
    Task<String> CreateOrderAsync(CreateOrderDto dto, string userId);
    Task CancelOrderAsync(String orderId, string userId);
    Task<List<OrderCardDto>> GetUserOrders(string userId, int index);
    Task<NotifyDto> GetNotifyInfo(string storeId);
    Task<OrderDetailDto> GetOrderDetail(string userId, string orderId);
    Task<OrderDetailDto> GetOrderDetail(string orderId);
    Task<OrderPagingResponse> GetPagingOrder(GetPagingOrderDto dto);
    Task<NotifyDto> NotifyPay(string orderId, string orderPaypalId);
    Task<NotifyDto> NotifyCancel(string orderId);
}
