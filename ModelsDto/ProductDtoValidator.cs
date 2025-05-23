using FluentValidation;

namespace marketplace_api.ModelsDto;

public class ProductDtoValidator : AbstractValidator<ProductDtoResponse>
{
    public ProductDtoValidator()
    {
        RuleFor(product => product.Name).NotEmpty()
            .Length(3, 50);
        RuleFor(product => product.Description).NotEmpty()
            .Length(10, 600);
        RuleFor(product => product.Price).NotEmpty()
            .LessThanOrEqualTo(1000000);
        RuleFor(product => product.Category).NotEmpty();
    }
}
