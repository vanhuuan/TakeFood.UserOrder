using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TakeFood.UserOrderService.ViewModel.Dtos.Voucher;

public class CreateVoucherDto
{
    /// <summary>
    /// Voucher name
    /// </summary>
    [JsonPropertyName("name")]
    [Required]
    public string Name { get; set; }
    /// <summary>
    /// Voucher description
    /// </summary>
    [JsonPropertyName("description")]
    [Required]
    public string Description { get; set; }
    [JsonPropertyName("minSpend")]
    [Required]
    public Double MinSpend { get; set; }
    [JsonPropertyName("amount")]
    [Required]
    public Double Amount { get; set; }
    [JsonPropertyName("maxDiscount")]
    [Required]
    public Double MaxDiscount { get; set; }
    [JsonPropertyName("StoreId")]
    [Required]
    public string StoreId { get; set; }
    [JsonPropertyName("code")]
    [Required]
    public string Code { get; set; }
    [JsonPropertyName("expireDay")]
    [Required]
    public DateTime ExpireDay { get; set; }
    [JsonPropertyName("startDay")]
    [Required]
    public DateTime StartDay { get; set; }
}

public class UpdateVoucherDto
{
    /// <summary>
    /// Voucher Id
    /// </summary>
    [JsonPropertyName("voucherID")]
    [Required]
    public string VoucherId { get; set; }
    /// <summary>
    /// Voucher name
    /// </summary>
    [JsonPropertyName("name")]
    [Required]
    public string Name { get; set; }
    /// <summary>
    /// Voucher description
    /// </summary>
    [JsonPropertyName("description")]
    [Required]
    public string Description { get; set; }
    [JsonPropertyName("minSpend")]
    [Required]
    public Double MinSpend { get; set; }
    [JsonPropertyName("amount")]
    [Required]
    public Double Amount { get; set; }
    [JsonPropertyName("maxDiscount")]
    [Required]
    public Double MaxDiscount { get; set; }
    [JsonPropertyName("StoreId")]
    [Required]
    public string StoreId { get; set; }
    [JsonPropertyName("code")]
    [Required]
    public string Code { get; set; }
    [JsonPropertyName("expireDay")]
    [Required]
    public DateTime ExpireDay { get; set; }
    [JsonPropertyName("startDay")]
    [Required]
    public DateTime StartDay { get; set; }
}
