using TakeFood.UserOrderService.ViewModel.Dtos.Address;

namespace TakeFood.UserOrderService.Service
{
    public interface IAddressService
    {
        Task CreateAddress(AddressDto address);
        Task DeleteAddress(String id);
    }
}
