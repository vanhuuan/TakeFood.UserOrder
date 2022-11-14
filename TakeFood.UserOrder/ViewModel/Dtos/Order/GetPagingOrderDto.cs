using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace TakeFood.UserOrder.ViewModel.Dtos.Order;

public class GetPagingOrderDto
{
    [Required]
    [FromQuery]
    public string UserId { get; set; }
    [Required]
    [FromQuery]
    public int PageNumber { get; set; }
    [Required]
    public int PageSize { get; set; }
    [FromQuery]
    public double From { get; set; }
    [FromQuery]
    public double To { get; set; }
    [FromQuery]
    public DateTime CreatedFrom { get; set; }
    [FromQuery]
    public DateTime CreatedTo { get; set; }
    /// <summary>
    /// Code/Name
    /// </summary>
    [Required]
    [FromQuery]
    public String StateType { get; set; }
    /// <summary>
    /// CreateDate StartDate EndDate Name Code
    /// </summary>
    [Required]
    [FromQuery]
    public String SortBy { get; set; }
    /// <summary>
    /// Desc Asc
    /// </summary>
    [Required]
    [FromQuery]
    public String SortType { get; set; }
}
