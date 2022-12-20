using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using StoreService.Service;
using System.ComponentModel.DataAnnotations;
using TakeFood.UserOrder.Hubs;
using TakeFood.UserOrder.Service;
using TakeFood.UserOrder.ViewModel.Dtos;
using TakeFood.UserOrder.ViewModel.Dtos.Order;
using TakeFood.UserOrderService.Controllers;

namespace TakeFood.UserOrder.Controllers;

public class OrderController : BaseController
{
    public IOrderService OrderService { get; set; }
    public IJwtService JwtService { get; set; }
    private readonly IHubContext<NotificationHub> notificationUserHubContext;
    public OrderController(IOrderService orderService, IJwtService jwtService, IHubContext<NotificationHub> hubContext)
    {
        OrderService = orderService;
        JwtService = jwtService;
        this.notificationUserHubContext = hubContext;
    }

    [HttpPost]
    [Route("CreateOrder")]
    public async Task<IActionResult> AddOrderAsync([FromBody] CreateOrderDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.ErrorCount);
            }
            var url = await OrderService.CreateOrderAsync(dto, GetId());
            return Ok(url);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPut]
    [Route("CancelOrder")]
    public async Task<IActionResult> CancelOrderAsync([Required] string orderId)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            await OrderService.CancelOrderAsync(orderId, GetId());
            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet]
    [Route("NotifyPay")]
    public async Task<IActionResult> NotifyPayAsync([Required] string orderId, [Required] string orderPaypalId)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var rs = await OrderService.NotifyPay(orderId, orderPaypalId);
            foreach (var connectionId in NotificationHub._connections.GetConnections(rs.UserId))
            {
                await notificationUserHubContext.Clients.Client(connectionId).SendAsync("sendToUser", rs.Header, rs.Message);
            }
            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet]
    [Route("NotifyCancel")]
    public async Task<IActionResult> NotifyCancelAsync([Required] string orderId)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var rs = await OrderService.NotifyCancel(orderId);
            foreach (var connectionId in NotificationHub._connections.GetConnections(rs.UserId))
            {
                await notificationUserHubContext.Clients.Client(connectionId).SendAsync("sendToUser", rs.Header, rs.Message);
            }
            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet]
    [Route("GetOrders")]
    public async Task<IActionResult> GetOrdersAsync([Required] int index)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var rs = await OrderService.GetUserOrders(GetId(), index);
            return Ok(rs);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet]
    [Route("GetOrderdetail")]
    public async Task<IActionResult> GetOrderDetailAsync([Required] string orderId)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var rs = await OrderService.GetOrderDetail(GetId(), orderId);
            return Ok(rs);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet]
    [Route("Notify")]
    public async Task<IActionResult> NotifyOrderStateChangeAsync([Required] string orderId)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var rs = await OrderService.GetNotifyInfo(orderId);
            foreach (var connectionId in NotificationHub._connections.GetConnections(rs.UserId))
            {
                await notificationUserHubContext.Clients.Client(connectionId).SendAsync("sendToUser", rs.Header, rs.Message);
            }
            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet]
    [Route("GetOrderPaging")]
    public async Task<IActionResult> GetOrderAdmin(GetPagingOrderDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var rs = await OrderService.GetPagingOrder(dto);
            return Ok(rs);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet]
    [Route("GetOrderAdminDetail")]
    public async Task<IActionResult> GetOrderAdminDetailAsync([Required] string orderId)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var rs = await OrderService.GetOrderDetail(orderId);
            return Ok(rs);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    public string GetId()
    {
        String id = HttpContext.Items["Id"]!.ToString()!;
        return id;
    }
    public string GetId(string token)
    {
        return JwtService.GetId(token);
    }
}
