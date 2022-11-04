using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TakeFood.UserOrder.ViewModel.Dtos;

public class CreateOrderDto
{
    /// <summary>
    /// Display Name
    /// </summary>
    [JsonPropertyName("address")]
    public string? Address { get; set; }
    /// <summary>
    /// Display Name
    /// </summary>
    [JsonPropertyName("addressId")]
    public string? AddressId { get; set; }
    /// <summary>
    /// Display Name
    /// </summary>
    [JsonPropertyName("note")]
    [Required]
    public string Note { get; set; }
    /// <summary>
    /// Display Name
    /// </summary>
    [JsonPropertyName("phoneNumber")]
    [Required]
    public string PhongeNumber { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("voucherId")]
    [Required]
    public string VoucherId { get; set; }
    /// <summary>
    /// Payment method
    /// </summary>
    [JsonPropertyName("paymentMethod")]
    [Required]
    public string PaymentMethod { get; set; }
    /// <summary>
    /// Delivery mode
    /// </summary>
    [JsonPropertyName("deliveryMode")]
    [Required]
    public string DeliveryMode { get; set; }
    /// <summary>
    /// Store id
    /// </summary>
    [JsonPropertyName("storeId")]
    [Required]
    public string StoreId { get; set; }

    /// <summary>
    /// List food
    /// </summary>
    [JsonPropertyName("listFood")]
    [Required]
    public List<FoodItem> ListFood { get; set; }
}

public class FoodItem
{
    /// <summary>
    /// Food id
    /// </summary>
    [JsonPropertyName("foodId")]
    [Required]
    public string FoodId { get; set; }
    /// <summary>
    /// List topping
    /// </summary>
    [JsonPropertyName("listTopping")]
    [Required]
    public List<ToppingItem> ListToppings { get; set; }
    /// <summary>
    /// Food quantity
    /// </summary>
    [JsonPropertyName("quantity")]
    [Required]
    public int Quantity
    {
        get; set;
    }
}

public class ToppingItem
{
    /// <summary>
    /// Topping id
    /// </summary>
    [JsonPropertyName("toppingId")]
    [Required]
    public string ToppingId { get; set; }
    /// <summary>
    /// Topping quantity
    /// </summary>
    [JsonPropertyName("quantity")]
    [Required]
    public int Quantity
    {
        get; set;
    }
}
