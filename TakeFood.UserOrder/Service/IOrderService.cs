using System.Runtime.CompilerServices;
using TakeFood.UserOrder.ViewModel.Dtos;

namespace TakeFood.UserOrder.Service;

public interface IOrderService
{
    Task CreateOrderAsync(CreateOrderDto dto, string userId);
    Task CancelOrderAsync(String orderId, string userId);
}
