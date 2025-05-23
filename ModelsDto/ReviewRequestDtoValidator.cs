﻿using FluentValidation;

namespace marketplace_api.ModelsDto;

public class ReviewRequestDtoValidator : AbstractValidator<ReviewRequestDto>
{
    public ReviewRequestDtoValidator()
    {
        RuleFor(r => r.UserId).GreaterThanOrEqualTo(0)
            .WithMessage("айди пользователя не может быть равным 0");
        RuleFor(r => r.ProductId).NotEmpty();
        RuleFor(r => r.ProductEvaluation).NotEmpty();
        RuleFor(r => r.DateCreated).NotEmpty();
        RuleFor(r => r.Description).NotEmpty().Length(10, 200);
        RuleFor(r => r.Rating).NotEmpty().LessThanOrEqualTo(10);
    }
}
