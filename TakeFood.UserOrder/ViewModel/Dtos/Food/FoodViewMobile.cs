using System.Text.Json.Serialization;
using TakeFood.UserOrderService.ViewModel.Dtos.Topping;

namespace TakeFood.UserOrderService.ViewModel.Dtos.Food
{
    public class FoodViewMobile
    {
        [JsonPropertyName("FoodId")]
        public string FoodId { get; set; }
        [JsonPropertyName("Name")]
        public string Name { get; set; }

        [JsonPropertyName("Dscription")]
        public string Description { get; set; }

        [JsonPropertyName("UrlImage")]
        public string UrlImage { get; set; }

        [JsonPropertyName("Price")]
        public double Price { get; set; }

        [JsonPropertyName("Category")]
        public string Category { get; set; }

        [JsonPropertyName("ListTopping")]
        public List<ToppingViewDto> ListTopping { get; set; }

        [JsonPropertyName("State")]
        public String State { get; set; }
    }
}
