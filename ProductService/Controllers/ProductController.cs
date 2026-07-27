// Controllers/ProductsController.cs
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private static readonly List<Product> _products = new()
    {
        new Product { Id = 1, Name = "Keyboard", Price = 49.99m, Stock = 100 },
        new Product { Id = 2, Name = "Mouse", Price = 19.99m, Stock = 200 }
    };

    [HttpGet]
    public ActionResult<IEnumerable<Product>> GetAll() => Ok(_products);

    [HttpGet("{id}")]
    public ActionResult<Product> GetById(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public ActionResult<Product> Create(Product product)
    {
        product.Id = _products.Max(p => p.Id) + 1;
        _products.Add(product);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPut("{id}/stock")]
    public IActionResult UpdateStock(int id, [FromBody] int newStock)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product is null) return NotFound();
        product.Stock = newStock;
        return NoContent();
    }
}