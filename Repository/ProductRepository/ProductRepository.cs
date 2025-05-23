﻿using marketplace_api.Data;
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
        return await _context.Products.Include(image => image.Images).ToListAsync();
    }

    public async Task<Product> GetByIdAsync(int id)
    {
        var product = await _context.Products
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
        var products = await _context.Products.Include(image => image.Images).Where(p => p.Name == name).ToListAsync();
        if (!products.Any())
        {
            throw new NotFoundExeption("Продуктов с данным именем не существует");
        }

        return products;
    }

    public  async Task<List<Product>> GetProductOdPage(int id)
    {
        int lengthPage = 10;
        int skip = (id - 1) * lengthPage;
        var page = await _context.Products.Include(image => image.Images)
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
