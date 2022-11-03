namespace StoreService.Model.Entities.Voucher;

public class Voucher : ModelMongoDB
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Double MinSpend { get; set; }
    public Double Amount { get; set; }
    /// <summary>
    /// Voucher type: True is shop, false is system
    /// </summary>
    public bool Type { get; set; }
    public Double MaxDiscount { get; set; }
    public string StoreId { get; set; }
    public string Code { get; set; }
    public DateTime ExpireDay { get; set; }
    public DateTime StartDay { get; set; }
}
