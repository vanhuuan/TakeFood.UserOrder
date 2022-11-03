namespace TakeFood.UserOrderService.ViewModel.Dtos;

public class GetPagingDto
{
    public int PageIndex { get; set; }
    public int TotalCount { get; set; }
    public string PageSize { get; set; }
}
