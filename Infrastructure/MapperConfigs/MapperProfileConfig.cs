using AutoMapper;
using Application.DTOs;
using Domain.Entities;
using System.Linq;

namespace Infrastructure.MapperConfigs
{
    public class MapperProfileConfig : Profile
    {
        public MapperProfileConfig()
        {
            CreateMap<ProductDetail, ProductDetailDTO>();

            // Account -> AccountDTO
            CreateMap<Account, AccountDTO>();

            // Product -> ProductDTO
            CreateMap<Product, ProductDTO>()
                .ForMember(d => d.Name, o => o.MapFrom(s => s.ProductName))
                .ForMember(d => d.Category, o => o.MapFrom(s => s.CategoryName))
                .ForMember(d => d.Stock, o => o.MapFrom(s => s.StockQuantity))
                .ForMember(d => d.ImageUrl, o => o.MapFrom(s => s.ProductDetails != null ? s.ProductDetails.Select(pd => pd.ImageUrl).FirstOrDefault() : null))
                .ForMember(d => d.IsAvailable, o => o.MapFrom(s => !s.IsDeleted));
        }
    }
}
