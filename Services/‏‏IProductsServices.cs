using Entities;
using DTOs;
namespace Services
{
    public interface IProductsServices
    {
        public Task<PageResponseDTO<ProductDTO>> GetProducts(int position, int skip, int?[] categoryIds,
          string? description, int? maxPrice, int? minPrice);
        public Task<IEnumerable<ProductDTO>> GetProducts();
        public Task<ProductDTO?> GetProductById(int id);
        public Task<ProductDTO> AddProduct(ProductDTO productDto);
        public Task<ProductDTO?> UpdateProduct(ProductDTO productDto);
        public Task<bool> DeleteProduct(int id);
    }
}