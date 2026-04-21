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

            // Shop -> ShopDTO
            CreateMap<Shop, ShopDTO>()
                .ForMember(d => d.OwnerName, o => o.MapFrom(s => s.Owner != null ? s.Owner.FullName ?? s.Owner.UserName : ""))
                .ForMember(d => d.ProductCount, o => o.MapFrom(s => s.Products != null ? s.Products.Count(p => !p.IsDeleted) : 0));

            // ShopApplication -> ShopApplicationDTO
            CreateMap<ShopApplication, ShopApplicationDTO>()
                .ForMember(d => d.ApplicantName, o => o.MapFrom(s => s.Applicant != null ? s.Applicant.FullName ?? s.Applicant.UserName : ""))
                .ForMember(d => d.ApplicantEmail, o => o.MapFrom(s => s.Applicant != null ? s.Applicant.Email : ""));
        }
    }
}
