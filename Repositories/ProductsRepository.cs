using Entities;
using Microsoft.EntityFrameworkCore;
using Repositories;
using System.Text.Json;
namespace Repositories


{

    public class ProductsRepository : IProductsRepository
    {

        public readonly ApiShopContext _context;
        public ProductsRepository(ApiShopContext context)
        {
            _context = context;
        }
        public async Task<(List<Product> Items, int TotalCount)> GetProducts(int position, int skip, int?[] categoryIds,
            string? description, int? maxPrice, int? minPrice)
        {
            var query = _context.Products.Where(product =>
                (description == null ? (true) : (product.Description.Contains(description))) &&
                ((maxPrice == null) ? (true) : (product.Price <= maxPrice)) &&
                ((minPrice == null) ? (true) : (product.Price >= minPrice)) &&
                ((categoryIds.Length == 0) ? (true) : (categoryIds.Contains(product.CategoryId)))).OrderBy(product => product.Price);
            List<Product> products = await query.Skip((position - 1) * skip).Take(skip).Include(product => product.Category).ToListAsync();
            var total = await query.CountAsync();
            return (products, total);
        }
        public async Task<IEnumerable<Product>> GetProducts()
        {
            return await _context.Products.Include(product => product.Category).ToListAsync();

        }
        public async Task<Product?> GetProductById(int id)
        {
            return await _context.Products.Include(product => product.Category).FirstOrDefaultAsync(p => p.ProductId == id);
        }
    }
}