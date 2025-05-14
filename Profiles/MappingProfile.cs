using W3_test.Data.Entities;
using W3_test.Domain.Models;
using W3_test.Domain.DTOs;
using AutoMapper;

namespace W3_test.Profiles
{
	public class MappingProfile : Profile
	{
		public MappingProfile()
		{
			
			CreateMap<Book, BookEntity>().ReverseMap();
			CreateMap<Cart, CartEntity>().ReverseMap();
			CreateMap<CartItems, CartItemEntity>().ReverseMap();
			CreateMap<Order, OrderEntity>().ReverseMap();
			CreateMap<OrderItems, OrderItemEntity>().ReverseMap();
			CreateMap<BookEntity, BookDTO>().ReverseMap();


			CreateMap<AppUser, AppUser>().ReverseMap();

			
			CreateMap<OrderEntity, OrderDTO>()
				.ForMember(dest => dest.User, opt => opt.MapFrom(src => src.UserId))
				.ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount)) 
				.ReverseMap();

			
			CreateMap<AppUser, AppUserDTO>()
				.ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.UserName)) 
				.ReverseMap();
		}
	}
}
