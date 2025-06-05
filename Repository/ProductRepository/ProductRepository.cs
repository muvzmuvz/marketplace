using marketplace_api.Data;
using marketplace_api.Models;
using Microsoft.AspNetCore.JsonPatch;
using marketplace_api.CustomExeption;
using Microsoft.EntityFrameworkCore;

namespace marketplace_api.Repository.ProductRepository;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<Product> CreateAsync(Product product)
    {
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        return product;
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) 
        {
            throw new NotFoundExeption("продукт не найден");
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Product>> GetAllAsync()
    {
        return await _context.Products
             .Include(user => user.User)
            .Include(image => image.Images)
            .ToListAsync();
    }

    public async Task<Product> GetByIdAsync(int id)
    {
        var product = await _context.Products
            .Include(user => user.User)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (product == null)
        {
            throw new NotFoundExeption("Данного продукта нет");
        }
        product.CountViewProduct++;
        await _context.SaveChangesAsync();

        return product;
    }

    public async Task<ICollection<Product>> GetByName(string name)
    {
        var products = await _context.Products
        .Include(user => user.User)
        .Include(image => image.Images)
        .ToListAsync();

        return products
            .Where(p =>
                p.Name.Contains(name, StringComparison.OrdinalIgnoreCase) ||
                ComputeLevenshteinDistance(p.Name.ToLower(), name.ToLower()) <= 2)
            .ToList();
    }

    private int ComputeLevenshteinDistance(string s, string t)
    {
        int[,] d = new int[s.Length + 1, t.Length + 1];

        for (int i = 0; i <= s.Length; i++)
            d[i, 0] = i;

        for (int j = 0; j <= t.Length; j++)
            d[0, j] = j;

        for (int j = 1; j <= t.Length; j++)
            for (int i = 1; i <= s.Length; i++)
                if (s[i - 1] == t[j - 1])
                    d[i, j] = d[i - 1, j - 1];
                else
                    d[i, j] = Math.Min(Math.Min(
                        d[i - 1, j] + 1,     // удаление
                        d[i, j - 1] + 1),    // вставка
                        d[i - 1, j - 1] + 1); // замена

        return d[s.Length, t.Length];
    }

    public async Task<List<Product>> GetProductByManagerIdAsync(int managerId)
    {
        var products = await _context.Products
            .Include(user => user.User)
            .Include(image => image.Images)
            .Where(p => p.UserId == managerId)
            .ToListAsync();

        return products;
    }

    public  async Task<List<Product>> GetProductOdPage(int id)
    {
        int lengthPage = 10;
        int skip = (id - 1) * lengthPage;
        var page = await _context.Products
            .Include(user => user.User)
            .Include(image => image.Images)
            .Skip(skip)
            .Take(lengthPage)
            .ToListAsync();

        return page;
    }

    public async Task<Product> PatchAsync(JsonPatchDocument<Product> productDto,int id)
    {
        var product = await _context.Products.FindAsync(id);
        if(product == null)
        {
            throw new NotFoundExeption("Данного пользователя нет");
        }

        productDto.ApplyTo(product);
        await _context.SaveChangesAsync();

        return product;
    }

    public async Task<Product> UpdateAsync(Product newProduct,int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            throw new NotFoundExeption("продукт не найден");
        }

        product.CountProduct = newProduct.CountProduct;
        product.Name = newProduct.Name;
        product.Description = newProduct.Description;
        product.Category = newProduct.Category;
        product.Price = newProduct.Price;
        product.Characteristic = newProduct.Characteristic;
        await _context.SaveChangesAsync();

        return newProduct;
    }  
}
