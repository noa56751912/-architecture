using Entities;
using Repositories;
using DTOs;
using AutoMapper;
using StackExchange.Redis;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
namespace Services
{
    public class ProductsServices : IProductsServices
    {
        private readonly IProductsRepository _repository;
        private readonly IMapper _mapper;
        private readonly IDatabase _redisDb;
        private readonly int _ttlMinutes;

        private const string CacheKey = "products:all";

        public ProductsServices(IProductsRepository repository, IMapper mapper, IConnectionMultiplexer redis, IConfiguration configuration)
        {
            _repository = repository;
            _mapper = mapper;
            _redisDb = redis.GetDatabase();
            _ttlMinutes = configuration.GetValue<int>("Redis:TtlMinutes", 5);
        }
        public async Task<PageResponseDTO<ProductDTO>> GetProducts(int position, int skip, int?[] categoryIds,
           string? description, int? maxPrice, int? minPrice)
        {

            (List<Product>, int) response = await _repository.GetProducts(position, skip, categoryIds, description, maxPrice, minPrice);
            List<ProductDTO> data = _mapper.Map<List<Product>, List<ProductDTO>>(response.Item1);
            PageResponseDTO<ProductDTO> pageResponse = new();
            pageResponse.Data = data;
            pageResponse.TotalItems = response.Item2;
            pageResponse.CurrentPage = position;
            pageResponse.PageSize = skip;
            pageResponse.HasPreviousPage = position > 1;
            int numOfPages = skip > 0 ? pageResponse.TotalItems / skip : 0;
            if (skip > 0 && pageResponse.TotalItems % skip != 0)
                numOfPages++;
            pageResponse.HasNextPage = position < numOfPages;
            return pageResponse;

        }
        public async Task<IEnumerable<ProductDTO>> GetProducts()
        {
            var cached = await _redisDb.StringGetAsync(CacheKey);
            if (cached.HasValue)
            {
                return JsonSerializer.Deserialize<IEnumerable<ProductDTO>>(cached!)!;
            }

            var products = _mapper.Map<IEnumerable<Product>, IEnumerable<ProductDTO>>(await _repository.GetProducts());
            if (products != null && products.Any())
            {
                await _redisDb.StringSetAsync(CacheKey, JsonSerializer.Serialize(products), TimeSpan.FromMinutes(_ttlMinutes));
            }
            return products ?? Enumerable.Empty<ProductDTO>();
        }

        
        public async Task<ProductDTO?> GetProductById(int id)
        {
            var product = await _repository.GetProductById(id);
            if (product == null) return null;
            return _mapper.Map<ProductDTO>(product);
        }

        public async Task<ProductDTO> AddProduct(ProductDTO productDto)
        {
            var product = _mapper.Map<Product>(productDto);
            var created = await _repository.AddProduct(product);
            await ClearCache();
            return _mapper.Map<ProductDTO>(created);
        }

        public async Task<ProductDTO?> UpdateProduct(ProductDTO productDto)
        {
            var product = _mapper.Map<Product>(productDto);
            var updated = await _repository.UpdateProduct(product);
            if (updated == null) return null;
            await ClearCache();
            return _mapper.Map<ProductDTO>(updated);
        }

        public async Task<bool> DeleteProduct(int id)
        {
            var deleted = await _repository.DeleteProduct(id);
            if (deleted) await ClearCache();
            return deleted;
        }

        private async Task ClearCache()
        {
            await _redisDb.KeyDeleteAsync(CacheKey);
        }
    }
}