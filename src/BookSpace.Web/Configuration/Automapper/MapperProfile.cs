using AutoMapper;
using BookSpace.Factories.ResponseModels;
using BookSpace.Models;
using BookSpace.Web.Areas.Admin.Models.ApplicationUserViewModels;
using BookSpace.Web.Models.BookViewModels;
using BookSpace.Web.Models.CommentViewModels;

namespace BookSpace.Web.Configuration.Automapper
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<ApplicationUser, ApplicationUserViewModel>().ReverseMap();
            CreateMap<Book, ListBookViewModel>().ReverseMap();
            CreateMap<Book, DetailedBookViewModel>().ReverseMap();

            CreateMap<Book, BookResponseModel>();
            CreateMap<Comment, CommentViewModel>().ForMember(c => c.Author, a => a.MapFrom(u => u.User.UserName));
            CreateMap<Comment, CommentResponseModel>().ForMember(c => c.Author, a => a.MapFrom(u => u.User.UserName));
            CreateMap<Genre, GenreResponseModel>();
            CreateMap<Tag, TagResponseModel>();


        }

    }
}
