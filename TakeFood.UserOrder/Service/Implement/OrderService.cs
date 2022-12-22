using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using StoreService.Model.Entities.Address;
using StoreService.Model.Entities.Food;
using StoreService.Model.Entities.Order;
using StoreService.Model.Entities.Store;
using StoreService.Model.Entities.Topping;
using StoreService.Model.Entities.Voucher;
using StoreService.Model.Repository;
using TakeFood.UserOrder.ViewModel.Dtos;
using TakeFood.UserOrder.ViewModel.Dtos.Order;
using Order = StoreService.Model.Entities.Order.Order;
using OrderPaypal = PayPalCheckoutSdk.Orders.Order;
using PaypalResponse = PayPalHttp.HttpResponse;

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
    private readonly PayPalHttpClient PaypalClient;
    private static String clientId = "AY9Ht8HTXWGsrpY02HujZA1NDBWXeakgmEWjgA0vY864tGe2YNfC6HWErCJ4xcxupMEblg5hZcX8Ihbv";
    private static String secret = "ECb4AWb-kGX8Leuu3ReilEgkCj9md1q81aYeGVlC-QlKN0hITRsrHKmy2hRctZzLO5HdDvMxhW1Ltna2";
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
        PayPalEnvironment environment = new SandboxEnvironment(clientId, secret);
        PaypalClient = new PayPalHttpClient(environment);
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
        await NotifyAsync(order.Id);
    }

    public async Task<String> CreateOrderAsync(CreateOrderDto dto, string userId)
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
        order.Sate = "Ordered";
        order.Mode = "1";
        order.PhoneNumber = dto.PhongeNumber;
        order.Note = dto.Note;
        order.StoreId = dto.StoreId;
        order.ReceiveTime = DateTime.MinValue;
        order.PaymentMethod = dto.PaymentMethod;
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
        if (voucher != null && money >= voucher.MinSpend && voucher.StartDay <= DateTime.UtcNow && voucher.ExpireDay >= DateTime.UtcNow)
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
        await NotifyAsync(order.Id);
        string paymentUrl = "";
        if (dto.PaymentMethod == "Paypal")
        {
            order.PaymentMethod = "Paypal - Chưa thanh toán";
            paymentUrl = await CreateOrderPaypalAsync(order.Total, order.Id);
            await orderRepository.UpdateAsync(order);
        }
        return paymentUrl;
    }

    private async Task<String> ConvertToUSD(double amount)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("apikey", "534GZaethTRRqHUR6OdTV2fyFiVbwn3E");
        HttpResponseMessage responseEx = await httpClient.GetAsync("https://api.apilayer.com/exchangerates_data/convert?to=USD&from=VND&amount=" + amount);

        if (!responseEx.IsSuccessStatusCode)
        {
            throw new Exception("Payment failed");
        }
        JObject json = JObject.Parse(await responseEx.Content.ReadAsStringAsync());
        return String.Format("{0:0.0}", Double.Parse(json["result"].ToString()));
    }

    private async Task<String> CreateOrderPaypalAsync(double amount, string orderId)
    {
        PaypalResponse response;
        var order = new OrderRequest()
        {
            CheckoutPaymentIntent = "CAPTURE",
            PurchaseUnits = new List<PurchaseUnitRequest>()
                {
                    new PurchaseUnitRequest()
                    {
                        AmountWithBreakdown = new AmountWithBreakdown()
                        {
                            CurrencyCode = "USD",
                            Value = await ConvertToUSD(amount)
                        }
                    }
                },
            ApplicationContext = new ApplicationContext()
            {
                ReturnUrl = "https://okmanga.web.app/notifySuccess?orderId=" + orderId,
                CancelUrl = "https://okmanga.web.app/notifyFailed?orderId=" + orderId
            }
        };

        var request = new OrdersCreateRequest();
        request.Prefer("return=representation");
        request.RequestBody(order);
        response = await PaypalClient.Execute(request);
        var statusCode = response.StatusCode;
        OrderPaypal result = response.Result<OrderPaypal>();
        foreach (LinkDescription link in result.Links)
        {
            if (link.Rel == "approve")
            {
                return link.Href;
            }
        }
        return "";
    }

    private async Task NotifyAsync(string orderId)
    {
        using var client = new HttpClient();
        var result = await client.GetAsync("https://takefood-orderservice.azurewebsites.net/api/Order/Notify?orderId=" + orderId);
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
            case "Processing":
                {
                    message = "Cửa hàng đã xác nhận";
                    break;
                }
            case "Delivering":
                {
                    message = "Đơn hàng đã sẵn sàng để giao/ lấy";
                    break;
                }
            case "Delivered":
                {
                    message = "Đơn hàng đã hoàn tất";
                    break;
                }
            default: message = "Đơn hàng đã cập nhật trạng thái"; break;
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
        var store = await storeRepository.FindByIdAsync(order.StoreId);
        var details = new OrderDetailDto();
        details.State = order.Sate;
        details.Note = order.Note;
        details.OrderId = order.Id;
        details.PhoneNumber = order.PhoneNumber;
        var address = await addressRepository.FindByIdAsync(order.AddressId);
        details.Address = address.Addrress;
        details.Total = order.Total;
        details.PaymentMethod = order.PaymentMethod;
        details.StoreName = store != null ? store.Name : "Cửa hàng không tồn tại";
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
        details.Discount = order.Discount;
        details.OrderDate = order.CreatedDate != null ? order.CreatedDate.Value.AddHours(+7) : DateTime.MinValue;
        return details;

    }

    public async Task<OrderPagingResponse> GetPagingOrder(GetPagingOrderDto dto)
    {
        var filter = CreateOrderFilter(dto.From, dto.To, dto.UserId, dto.StateType, dto.CreatedFrom, dto.CreatedTo);
        var sort = CreateSortFilter(dto.SortType, dto.SortBy);
        var rs = await orderRepository.GetPagingAsync(filter, dto.PageNumber - 1, dto.PageSize, sort);
        var list = new List<OrderAdminCard>();
        foreach (var order in rs.Data)
        {
            var address = await addressRepository.FindByIdAsync(order.AddressId);
            var addressString = address == null ? "Lỗi" : address.Addrress;
            list.Add(new OrderAdminCard()
            {
                OrderId = order.Id,
                Address = addressString,
                OrderDate = order.CreatedDate!.Value,
                PhoneNumber = order.PhoneNumber,
                State = order.Sate,
                Total = order.Total
            });
        }
        int stt = 0;
        foreach (var i in list)
        {
            stt++;
            i.Id = stt;
            i.Stt = stt;
        }
        var info = new OrderPagingResponse()
        {
            Total = rs.Count,
            PageIndex = dto.PageNumber,
            PageSize = dto.PageSize,
            Cards = list
        };
        return info;
    }

    private FilterDefinition<Order> CreateOrderFilter(double from, double to, string uid, string orderState, DateTime startDate, DateTime endDate)
    {
        var filter = Builders<Order>.Filter.Eq(x => x.UserId, uid);
        filter &= Builders<Order>.Filter.Gte(x => x.Total, from);
        filter &= Builders<Order>.Filter.Lte(x => x.Total, to);
        if (!(startDate == DateTime.MinValue || startDate == DateTime.MinValue))
        {
            filter &= Builders<Order>.Filter.Gte(x => x.CreatedDate, startDate);
            filter &= Builders<Order>.Filter.Lte(x => x.CreatedDate, endDate);
        }
        if (orderState != "All")
        {
            filter &= Builders<Order>.Filter.Eq(x => x.Sate, orderState);
        }
        return filter;
    }

    public async Task<bool> CaptureOrderAsync(string orderPaypalId)
    {
        try
        {
            var request = new OrdersCaptureRequest(orderPaypalId);
            request.RequestBody(new OrderActionRequest());
            PaypalResponse response = await PaypalClient.Execute(request);
            var statusCode = response.StatusCode;
            OrderPaypal result = response.Result<OrderPaypal>();
            if (result.Status == "COMPLETED")
            {
                return true;
            }
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private SortDefinition<Order> CreateSortFilter(string sortType, string sortBy)
    {
        if (sortType == "Asc")
        {
            switch (sortBy)
            {
                case "OrderId": return Builders<Order>.Sort.Ascending(x => x.Id);
                case "Total": return Builders<Order>.Sort.Ascending(x => x.Total);
                case "CreateDate": return Builders<Order>.Sort.Ascending(x => x.CreatedDate);
                default: return Builders<Order>.Sort.Ascending(x => x.CreatedDate);
            }
        }
        else
        {
            switch (sortBy)
            {
                case "OrderId": return Builders<Order>.Sort.Descending(x => x.Id);
                case "Total": return Builders<Order>.Sort.Descending(x => x.Total);
                case "CreateDate": return Builders<Order>.Sort.Descending(x => x.CreatedDate);
                default: return Builders<Order>.Sort.Descending(x => x.CreatedDate);
            }
        }

    }

    public async Task<List<OrderCardDto>> GetUserOrders(string userId, int index)
    {
        var orders = new List<OrderCardDto>();
        FilterDefinition<Order> constrain = Builders<Order>.Filter.Where(x => x.UserId == userId);
        SortDefinition<Order> sort = Builders<Order>.Sort.Descending(x => x.CreatedDate);
        var listOrder = await orderRepository.GetPagingAsync(constrain, index - 1, 10, sort);
        foreach (var order in listOrder.Data)
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
                FoodQuantity = foodQuantity,
                Created = order.CreatedDate!.Value.AddHours(+7)
            });
        }
        return orders;
    }

    public async Task<OrderDetailDto> GetOrderDetail(string orderId)
    {
        var order = await orderRepository.FindByIdAsync(orderId);
        var store = await storeRepository.FindByIdAsync(order.StoreId);
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
        details.Discount = order.Discount;
        details.StoreName = store != null ? store.Name : "Cửa hàng đã bị xóa";
        details.OrderDate = order.CreatedDate != null ? order.CreatedDate.Value.AddHours(7) : DateTime.MinValue;
        return details;
    }

    public async Task<NotifyDto> NotifyPay(string orderId, string orderPaypalId)
    {
        var order = await orderRepository.FindByIdAsync(orderId);
        if (order == null)
        {
            throw new Exception("Order's not exist!");
        }
        if (await CaptureOrderAsync(orderPaypalId))
        {
            var dto = new NotifyDto()
            {
                UserId = order.UserId,
                Header = "Thanh tóan thành công",
                Message = "Thanh toán " + order.Total + " thành công!"
            };
            order.PaymentMethod = "Paypal - Thanh toán thành công";
            await orderRepository.UpdateAsync(order);
            return dto;
        }
        return new NotifyDto()
        {
            UserId = order.UserId,
            Header = "Thanh tóan chưa thành công",
            Message = "Thanh toán " + order.Total + " chưa thành công!"
        };

    }

    public async Task<NotifyDto> NotifyCancel(string orderId)
    {
        var order = await orderRepository.FindByIdAsync(orderId);
        if (order == null)
        {
            throw new Exception("Order's not exist!");
        }
        var dto = new NotifyDto()
        {
            UserId = order.UserId,
            Header = "Thanh tóan không thành công",
            Message = "Thanh toán " + order.Total + " không thành công!"
        };
        order.Sate = "Canceled";
        order.PaymentMethod = "Paypal - Thanh toán không thành công";
        await orderRepository.UpdateAsync(order);
        return dto;
    }
}
