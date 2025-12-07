using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartGearOnline.Models;
using SmartGearOnline.Services;
using System.Threading.Tasks;

namespace SmartGearOnline.Controllers
{
    [ApiController]
    [Route("api/products")]   // <- changed from "api/[controller]"
    public class ProductsApiController : ControllerBase
    {
        private readonly IProductService _service;
        private readonly ILogger<ProductsApiController> _logger;

        public ProductsApiController(IProductService service, ILogger<ProductsApiController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _service.GetAllProductsAsync();
            return Ok(items);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Product product)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            await _service.AddProductAsync(product);
            // If the service/repo sets Id, return CreatedAtAction; otherwise return Created with product
            return CreatedAtAction(nameof(Get), new { id = product.Id }, product);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] Product product)
        {
            if (id != product.Id) return BadRequest();
            var success = await _service.UpdateProductAsync(id, product);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteProductAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}