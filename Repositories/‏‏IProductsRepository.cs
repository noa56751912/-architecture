using Entities;

namespace Repositories
{
    public interface IProductsRepository
    {
        public Task<(List<Product> Items, int TotalCount)> GetProducts(int position, int skip, int?[] categoryIds,
         string? description, int? maxPrice, int? minPrice);
        public Task<IEnumerable<Product>> GetProducts();
        public Task<Product?> GetProductById(int id);
        public Task<Product> AddProduct(Product product);
        public Task<Product?> UpdateProduct(Product product);
        public Task<bool> DeleteProduct(int id);

    }
}