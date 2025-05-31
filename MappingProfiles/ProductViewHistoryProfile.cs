using AutoMapper;
using marketplace_api.Models;
using marketplace_api.ModelsDto;

public class ProductViewHistoryProfile : Profile
{
    public ProductViewHistoryProfile()
    {
        CreateMap<ProductViewHistory, ProductViewHsitoryDto>();
        CreateMap<ProductViewHsitoryDto, ProductViewHistory>();

    }
}