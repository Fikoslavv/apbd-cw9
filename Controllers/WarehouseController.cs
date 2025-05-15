using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace apbd_cw9;

[ApiController]
[Route("/warehouse")]
public class WarehouseController : ControllerBase
{
    protected readonly IService<Warehouse> warehouseService;
    protected readonly IService<ProductWarehouse> productWarehouseService;
    protected readonly IService<Product> productService;
    protected readonly IService<Order> orderService;

    public WarehouseController(IService<Warehouse> warehouseService, IService<ProductWarehouse> productWarehouseService, IService<Product> productService, IService<Order> orderService)
    {
        this.warehouseService = warehouseService;
        this.productWarehouseService = productWarehouseService;
        this.productService = productService;
        this.orderService = orderService;
    }

    [HttpGet("/warehouse/product")]
    public async Task<IActionResult> GetAllProductWarehousesAsync([FromQuery] IDictionary<string, string[]> filters) => await Task.Run(() => this.GetAllProductWarehouses(filters));

    protected IActionResult GetAllProductWarehouses(IDictionary<string, string[]> filters)
    {
        var querry = this.productWarehouseService.GetData(filters);
        return querry.Any() ? this.Ok(querry) : this.NotFound("No product_warehouse satisfies given conditions");
    }

    [HttpGet]
    public async Task<IActionResult> GetAllWarehousesAsync([FromQuery] IDictionary<string, string[]> filters) => await Task.Run(() => this.GetAllWarehouses(filters));

    protected IActionResult GetAllWarehouses(IDictionary<string, string[]> filters)
    {
        var querry = warehouseService.GetData(filters);
        return querry.Any() ? this.Ok(querry) : this.NotFound("No warehouse satisfies given conditions.");
    }

    [HttpPut]
    public async Task<IActionResult> PutWarehouseAsync(Warehouse warehouse) => await Task.Run(() => this.PutWarehouse(warehouse));

    protected IActionResult PutWarehouse(Warehouse warehouse)
    {
        return this.warehouseService.InsertData(warehouse) ? this.Ok(warehouse) : this.Conflict("Failed to put a warehouse.");
    }

    [HttpPut("/warehouse/product")]
    public async Task<IActionResult> PutProductWarehouseAsync(ProductWarehouse value, [FromQuery] bool runStoredProcedure = true) => await Task.Run(() => this.PutProductWarehouse(value, runStoredProcedure));

    private IActionResult PutProductWarehouse(ProductWarehouse value, bool runStoredProcedure)
    {
        if (runStoredProcedure)
        {
            var idProductWarehouse = this.warehouseService.RunStoredProcedure
            (
                "AddProductToWarehouse",
                (command) => (int?)Convert.ToInt32(command.ExecuteScalar()),
                new KeyValuePair<string, object>(nameof(value.IdProduct), value.IdProduct),
                new KeyValuePair<string, object>(nameof(value.IdWarehouse), value.IdWarehouse),
                new KeyValuePair<string, object>(nameof(value.Amount), value.Amount),
                new KeyValuePair<string, object>(nameof(value.CreatedAt), value.CreatedAt)
            );

            return idProductWarehouse is not null ? this.Ok(new { IdProductWarehouse = idProductWarehouse }) : this.Conflict("Failed to put a product_warehouse.");
        }
        else
        {
            var product = this.productService.GetData([ new(nameof(Product.IdProduct), [ value.IdProduct.ToString() ]) ]).SingleOrDefault();
            var order = this.orderService.GetData([ new(nameof(Order.IdProduct), [ value.IdProduct.ToString() ]), new(nameof(Order.Amount), [ value.Amount.ToString() ]) ]).Where(o => o.CreatedAt < value.CreatedAt && o.FulfilledAt is null).FirstOrDefault();
    
            if (product is null) return this.NotFound("No product with given id was found.");
            else if (!this.warehouseService.GetData([ new(nameof(Warehouse.IdWarehouse), [ value.IdWarehouse.ToString() ]) ]).Any()) return this.NotFound("No warehouse with given id was found.");
            else if (value.Amount <= 0) return this.BadRequest("Amount cannot be less then or equal to zero.");
            else if (order is null) return this.NotFound("No order was found with given id and amount that would have creation date from before given date of creation and that would not be already fulfilled.");
            else if (this.productWarehouseService.GetData([ new(nameof(ProductWarehouse.IdOrder), [ order.IdOrder.ToString() ]) ]).Any()) return this.Conflict("There already is a product_warehouse that fulfills all applicable orders.");
    
            value.Price = value.Amount * product.Price;
            value.CreatedAt = DateTime.Now;
            value.IdOrder = order.IdOrder;
    
            order.FulfilledAt = DateTime.Now;
    
            using SqlConnection connection = this.warehouseService.GetNewConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();
    
            if (this.orderService.UpdateData(order, connection, transaction) && this.productWarehouseService.InsertData(value, connection, transaction))
            {
                transaction.Commit();
                return this.Ok(new { IdProductWarehouse = value.IdProductWarehouse });
            }
            else
            {
                transaction.Rollback();
                return this.Conflict("Failed to put a product_warehouse.");
            }
        }
    }
}
