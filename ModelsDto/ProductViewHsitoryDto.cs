using marketplace_api.Models;

namespace marketplace_api.ModelsDto;

public class ProductViewHsitoryDto
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int ProductId { get; set; }
    public ProductDto Product { get; set; }

    public DateTime ViewDate { get; set; } = DateTime.UtcNow;
}
