using Microsoft.AspNetCore.Mvc;

namespace apbd_cw9;

[ApiController]
[Route("/order")]
public class OrderController : ControllerBase
{
    protected IService<Order> orderService;

    public OrderController(IService<Order> orderService)
    {
        this.orderService = orderService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllOrdersAsync([FromQuery] IDictionary<string, string[]> filters) => await Task.Run(() => this.GetAllOrders(filters));

    protected IActionResult GetAllOrders(IDictionary<string, string[]> filters)
    {
        var querry = this.orderService.GetData(filters);
        return querry.Any() ? this.Ok(querry) : this.NotFound("No order satisfies given conditions.");
    }

    [HttpGet("/order/{id}")]
    public async Task<IActionResult> GetOrderAsync(int id) => await Task.Run(() => this.GetOrder(id));

    private IActionResult GetOrder(int id)
    {
        var order = this.orderService.GetData([ new(nameof(Order.IdOrder), [ id.ToString() ]) ]).SingleOrDefault();
        return order is not null ? this.Ok(order) : this.NotFound("No order with given id was found.");
    }

    [HttpPut]
    public async Task<IActionResult> PutOrderAsync(Order order) => await Task.Run(() => this.PutOrder(order));

    protected IActionResult PutOrder(Order order)
    {
        return this.orderService.InsertData(order) ? this.Ok(order) : this.Conflict("Failed to put an order.");
    }
}
