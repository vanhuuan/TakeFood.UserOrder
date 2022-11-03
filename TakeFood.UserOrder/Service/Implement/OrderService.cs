using StoreService.Model.Entities.Address;
using StoreService.Model.Entities.Food;
using StoreService.Model.Entities.Order;
using StoreService.Model.Entities.Store;
using StoreService.Model.Entities.Topping;
using StoreService.Model.Repository;
using TakeFood.UserOrder.ViewModel.Dtos;

namespace TakeFood.UserOrder.Service.Implement;

public class OrderService : IOrderService
{
    private readonly IMongoRepository<Order> orderRepository;
    private readonly IMongoRepository<Food> foodRepository;
    private readonly IMongoRepository<FoodOrder> foodOrderRepository;
    private readonly IMongoRepository<Topping> toppingRepository;
    private readonly IMongoRepository<FoodTopping> foodToppingRepository;
    private readonly IMongoRepository<Address> addressRepository;
    private readonly IMongoRepository<Store> storeRepository;
    public OrderService(IMongoRepository<Order> orderRepository, IMongoRepository<Food> foodRepository, IMongoRepository<FoodOrder> foodOrderRepository,
        IMongoRepository<Topping> toppingRepository, IMongoRepository<FoodTopping> foodToppingRepository, IMongoRepository<Address> addressRepository)
    {
        this.orderRepository = orderRepository;
        this.addressRepository = addressRepository;
        this.foodRepository = foodRepository;
        this.foodOrderRepository = foodOrderRepository;
        this.foodToppingRepository = foodToppingRepository;
        this.foodRepository = foodRepository;
        this.toppingRepository = toppingRepository;
    }
    public Task CancelOrderAsync(string orderId, string userId)
    {
        throw new NotImplementedException();
    }

    public async Task CreateOrderAsync(CreateOrderDto dto, string userId)
    {
        if((await storeRepository.FindByIdAsync(dto.StoreId)) == null)
        {
            throw new Exception("Store's not exist!");
        }
        var order = new Order();
        order.UserId = userId;
        if (dto.AddressId == null || (await addressRepository.FindByIdAsync(dto.AddressId)) == null)
        {
            Address address = new Address()
            {
                AddressType = "Home",
                Addrress = dto.Address,
                Lat = 0,
                Lng = 0
            };
            address = await addressRepository.InsertAsync(address);
            dto.AddressId = address.Id;
        }
        order.AddressId = dto.AddressId;
        order.PaymentMethod = dto.PaymentMethod;
        order.Sate = "Ordered";
        order.Mode = "1";
        order.PhoneNumber = dto.PhongeNumber;
        order.Note = dto.Note;
        order.StoreId = dto.StoreId;

    }
}
