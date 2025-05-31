using marketplace_api.Models;

namespace marketplace_api.Repository.ProductViewHistoryRepository;

public interface IProductViewHistoryRepository
{
    Task<IEnumerable<ProductViewHistory>> GetAllHistory(int userId);
    Task AddProducthistory(int userId,Product product);
    Task UpdateProducthistory(int userId,Product product,int productId);
    Task DeleteProducthistory(int userId,int productId);
    Task<ProductViewHistory> GetProduct(int userId,int productId);
    Task RemoveAllHisory(int userId);
}
