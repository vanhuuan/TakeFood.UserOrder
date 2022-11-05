using MongoDB.Driver;
using StoreService.Model.Entities.Address;
using StoreService.Model.Entities.Food;
using StoreService.Model.Entities.Order;
using StoreService.Model.Entities.Store;
using StoreService.Model.Entities.Topping;
using StoreService.Model.Entities.Voucher;
using StoreService.Model.Repository;
using TakeFood.UserOrder.ViewModel.Dtos;
using TakeFood.UserOrder.ViewModel.Dtos.Order;

namespace TakeFood.UserOrder.Service.Implement;

public class OrderService : IOrderService
{
    private readonly IMongoRepository<Order> orderRepository;
    private readonly IMongoRepository<Food> foodRepository;
    private readonly IMongoRepository<FoodOrder> foodOrderRepository;
    private readonly IMongoRepository<ToppingOrder> toppingOrderRepository;
    private readonly IMongoRepository<Topping> toppingRepository;
    private readonly IMongoRepository<FoodTopping> foodToppingRepository;
    private readonly IMongoRepository<Address> addressRepository;
    private readonly IMongoRepository<Store> storeRepository;
    private readonly IMongoRepository<Voucher> voucherRepository;
    public OrderService(IMongoRepository<Order> orderRepository, IMongoRepository<Food> foodRepository, IMongoRepository<FoodOrder> foodOrderRepository,
        IMongoRepository<Topping> toppingRepository, IMongoRepository<FoodTopping> foodToppingRepository, IMongoRepository<Address> addressRepository, IMongoRepository<ToppingOrder> toppingOrderRepository, IMongoRepository<Store> storeRepository, IMongoRepository<Voucher> voucherRepository)
    {
        this.orderRepository = orderRepository;
        this.addressRepository = addressRepository;
        this.foodRepository = foodRepository;
        this.foodOrderRepository = foodOrderRepository;
        this.foodToppingRepository = foodToppingRepository;
        this.foodRepository = foodRepository;
        this.toppingRepository = toppingRepository;
        this.toppingOrderRepository = toppingOrderRepository;
        this.storeRepository = storeRepository;
        this.voucherRepository = voucherRepository;
    }
    public async Task CancelOrderAsync(string orderId, string userId)
    {
        var order = await orderRepository.FindByIdAsync(orderId);
        if (order == null || order.UserId != userId)
        {
            throw new Exception("Order's not exist!");
        }
        order.Sate = "Canceled";
        await orderRepository.UpdateAsync(order);
        // Call api to notify owner
    }

    public async Task CreateOrderAsync(CreateOrderDto dto, string userId)
    {
        var store = await storeRepository.FindByIdAsync(dto.StoreId);
        if (store == null)
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
        order.ReceiveTime = DateTime.MinValue;
        var foodsStoreId = foodRepository.FindAsync(x => x.StoreId == order.StoreId).Result.Select(x => x.Id);
        order = await orderRepository.InsertAsync(order);
        double money = 0;
        foreach (var foodItem in dto.ListFood)
        {
            if (foodItem.Quantity <= 0) continue;
            if (foodsStoreId.Contains(foodItem.FoodId))
            {
                var food = await foodRepository.FindByIdAsync(foodItem.FoodId);
                money += food.Price * foodItem.Quantity;
                var foodOrder = await foodOrderRepository.InsertAsync(new FoodOrder()
                {
                    FoodId = foodItem.FoodId,
                    OrderId = order.Id,
                    Quantity = foodItem.Quantity
                });
                var listFoodTopping = foodToppingRepository.FindAsync(x => x.FoodId == foodItem.FoodId).Result.Select(x => x.ToppingId);

                foreach (var toppingItem in foodItem.ListToppings)
                {
                    if (toppingItem.Quantity <= 0) continue;
                    if (listFoodTopping.Contains(toppingItem.ToppingId))
                    {
                        var topping = await toppingRepository.FindOneAsync(x => x.Id == toppingItem.ToppingId);
                        money += topping.Price * toppingItem.Quantity;
                        var toppingOrder = await toppingOrderRepository.InsertAsync(new ToppingOrder()
                        {
                            Quantity = toppingItem.Quantity,
                            ToppingId = topping.Id,
                            FoodOrderId = foodOrder.Id
                        });
                    }
                }
            }
        }
        var voucher = await voucherRepository.FindByIdAsync(dto.VoucherId);
        if (voucher != null && money >= voucher.MinSpend && voucher.StartDay <= DateTime.UtcNow && voucher.ExpireDay <= DateTime.UtcNow)
        {
            var discount = money * (voucher.Amount / 100);
            if (discount > voucher.MaxDiscount)
            {
                discount = voucher.MaxDiscount;
            }
            money = money - discount;
            order.Discount = discount;
        }
        order.Total = money;
        await orderRepository.UpdateAsync(order);
        // Call api to notify owner
    }

    public async Task<NotifyDto> GetNotifyInfo(string storeId)
    {
        var order = await orderRepository.FindByIdAsync(storeId);
        if (order == null)
        {
            throw new Exception("Order's not exist");
        }
        string message = "Cửa hàng đã xác nhận";
        switch (order.Sate)
        {
            case "Đã xác nhận":
                {
                    break;
                }
            case "Sẵn sàng":
                {
                    message = "Đơn hàng đã sẵn sáng để giao/ lấy";
                    break;
                }
            case "Hoàn tất":
                {
                    message = "Đơn hàng đã hoàn tất";
                    break;
                }
            default: message = "Message nay chi de test thoi"; break;
        }

        var dto = new NotifyDto()
        {
            UserId = order.UserId,
            Header = "Cập nhật trạng thái đơn hàng",
            Message = message
        };
        return dto;
    }

    public async Task<OrderDetailDto> GetOrderDetail(string userId, string orderId)
    {
        var order = await orderRepository.FindByIdAsync(orderId);
        if (order == null || order.UserId != userId)
        {
            throw new Exception("Order's note exist");
        }
        var details = new OrderDetailDto();
        details.State = order.Sate;
        details.Note = order.Note;
        details.OrderId = order.Id;
        details.PhoneNumber = order.PhoneNumber;
        var address = await addressRepository.FindByIdAsync(order.AddressId);
        details.Address = address.Addrress;
        details.Total = order.Total;
        details.PaymentMethod = order.PaymentMethod;
        var foodsOrder = await foodOrderRepository.FindAsync(x => x.OrderId == order.Id);
        var listfoods = new List<FoodDetailsItem>();
        foreach (var i in foodsOrder)
        {
            var foodDetailsItem = new FoodDetailsItem();
            var food = await foodRepository.FindByIdAsync(i.FoodId);
            if (food == null) continue;
            foodDetailsItem.FoodId = food.Id;
            foodDetailsItem.FoodName = food.Name;
            foodDetailsItem.Quantity = i.Quantity;
            var listTopping = new List<ToppingDetailsItem>();
            var toppingOrders = await toppingOrderRepository.FindAsync(x => x.FoodOrderId == i.Id);
            double total = food.Price * i.Quantity;
            foreach (var toppingOrder in toppingOrders)
            {
                var topping = await toppingRepository.FindByIdAsync(toppingOrder.ToppingId);
                if (topping == null) continue;
                var t = new ToppingDetailsItem()
                {
                    ToppingId = topping.Id,
                    ToppingName = topping.Name,
                    Total = topping.Price * toppingOrder.Quantity,
                    Quantity = toppingOrder.Quantity
                };
                listTopping.Add(t);
                total += topping.Price * toppingOrder.Quantity;
            }
            foodDetailsItem.Total = total;
            foodDetailsItem.Toppings = listTopping;
            listfoods.Add(foodDetailsItem);
        }
        details.Foods = listfoods;
        return details;

    }

    public async Task<List<OrderCardDto>> GetUserOrders(string userId, int index)
    {
        var orders = new List<OrderCardDto>();
        FilterDefinition<Order> constrain = Builders<Order>.Filter.Where(x => x.UserId == userId);
        var listOrder = orderRepository.FindAsync(x => x.UserId == userId).Result.Take(index * 10).TakeLast(10);
        foreach (var order in listOrder)
        {
            var store = await storeRepository.FindByIdAsync(order.StoreId);
            if (store == null) continue;
            var foodQuantity = await foodOrderRepository.CountAsync(x => x.OrderId == order.Id);
            orders.Add(new OrderCardDto()
            {
                OrderId = order.Id,
                State = order.Sate,
                StoreName = store.Name,
                Total = order.Total,
                FoodQuantity = foodQuantity
            });
        }
        return orders;
    }
}
