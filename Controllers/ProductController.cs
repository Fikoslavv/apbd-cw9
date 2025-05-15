using Microsoft.AspNetCore.Mvc;

namespace apbd_cw9;

[ApiController]
[Route("/product")]
public class ProductController : ControllerBase
{
    protected IService<Product> productService;

    public ProductController(IService<Product> productService)
    {
        this.productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllProductsAsync([FromQuery] IDictionary<string, string[]> fields) => await Task.Run(() => this.GetAllProducts(fields));

    protected IActionResult GetAllProducts(IDictionary<string, string[]> fields)
    {
        var querry = this.productService.GetData(fields);
        return querry.Any() ? this.Ok(querry) : this.NotFound("No product satisfies given conditions.");
    }

    [HttpPut]
    public async Task<IActionResult> PutProductAsync(Product product) => await Task.Run(() => this.PutProduct(product));

    private IActionResult PutProduct(Product product)
    {
        return this.productService.InsertData(product) ? this.Ok(product) : this.Conflict("Failed to put a product.");
    }
}
