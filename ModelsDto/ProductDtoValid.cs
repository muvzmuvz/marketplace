﻿using FluentValidation;
using marketplace_api.Models;

namespace marketplace_api.ModelsDto;

public class ProductDtoValid : AbstractValidator<ProductDto>
{
    public ProductDtoValid()
    {
        RuleFor(product => product.Name).NotEmpty()
                .Length(3, 100);
        RuleFor(product => product.Description).NotEmpty()
                .Length(10, 6000);
        RuleFor(product => product.Price).NotEmpty()
                .LessThanOrEqualTo(1000000);
        RuleFor(product => product.Category).NotEmpty();
    }
}
