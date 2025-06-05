using marketplace_api.Models;

namespace marketplace_api.Services.ProductViewHistoryService;

public interface IProductViewHistoryService
{
    Task<IEnumerable<ProductViewHistory>> GetAllHistoryAsync(int userId);
    Task<ProductViewHistory> GetHistoryAsync(int userId, int productId);
    Task AddHistoryAsync(int productId, int userId);
    Task UpdateHistoryAsync(Product product, int userId, int productId);
    Task DeleteHistoryAsync(int userId,int productId);
    Task RemoveAllHistory(int userId);
}
