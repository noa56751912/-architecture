using Microsoft.AspNetCore.Mvc;
using Services;
using DTOs;

namespace WebApiShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductsServices _IProductsServices;

        public ProductsController(IProductsServices productsServices)
        {
            _IProductsServices = productsServices;
        }
       
        [HttpGet ("Filter")]
        public async Task<ActionResult<PageResponseDTO<ProductDTO>>> Get(int position, int skip, [FromQuery] int?[] categoryIds, string? description, int? maxPrice, int? minPrice)
        {

            PageResponseDTO<ProductDTO> pageResponse = await _IProductsServices.GetProducts(position, skip, categoryIds, description, maxPrice, minPrice);
            if (pageResponse.Data != null && pageResponse.Data.Count > 0)
                return Ok(pageResponse);
            return NoContent();
        }



        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> Get()
        {
            IEnumerable<ProductDTO> products = await _IProductsServices.GetProducts();
            if (products != null && products.Any())
                return Ok(products);
            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDTO>> GetById(int id)
        {
            ProductDTO? product = await _IProductsServices.GetProductById(id);
            if (product == null)
                return NotFound();
            return Ok(product);
        }

    }
}


