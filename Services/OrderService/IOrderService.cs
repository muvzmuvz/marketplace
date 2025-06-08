using marketplace_api.Models;
using marketplace_api.ModelsDto;

namespace marketplace_api.Services.OrderService;

public interface IOrderService
{
    Task CreateOrderAsync(CreateOrderRequest orderRequest);
    Task<List<OrderDto>> GetOrdersAsync(int userId);
    Task<OrderDto> GetOrderByIdAsync(int orderId);
    Task RemoveOrderFromDeliveryAsync(int orderId);
    Task OrderUpdateOfStatusAsync(OrderStatus orderStatus, int orderid);
    Task<List<OrderDto>> GetOrdersByDate(DateTime startDate, DateTime endDate);
    Task UpdateOrderAsync(OrderDto newOrder, int orderId);
    Task<List<OrderDto>> GetOrdersOfSeller(int sellerId);
}
