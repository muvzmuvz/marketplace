﻿using marketplace_api.Models;

namespace marketplace_api.ModelsDto;

public class ProductDto
{
    public int CountProduct { get; set; }
    public Category Category { get; set; }
    public decimal Price { get; set; } = decimal.Zero;
    public string Name { get; set; }
    public string Description { get; set; }
    public int UserId { get; set; }
    public UserDto User { get; set; }
    public int Id { get; set; }
    public string imagePath { get; set; }
    public string Characteristic { get; set; }
    public List<Image> Images { get; set; }
}