using AutoMapper;
using RandomShop.Data.Models;
using RandomShop.Models.Category;

namespace RandomShop.Infrastructure
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            this.CreateMap<CategoryFormModel, Category>();
            this.CreateMap<Category, CategoryFormModel>();
            this.CreateMap<Category, CategoryViewModel>();
            this.CreateMap<Category, SubCategoryModel>();
        }
    }
}
