using CapstoneProjectBlog.Models;
using AutoMapper;
using CapstoneProjectBlog.Dtos;
using System;

namespace CapstoneProjectBlog.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<UserModel, UserAddDto>();
            CreateMap<UserModel, UserEditDto>();
            CreateMap<UserAddDto, UserModel>();
            CreateMap<UserEditDto, UserModel>();
            CreateMap<RoleModel, RoleAddDto>();
            CreateMap<RoleModel, RoleEditDto>();
            CreateMap<RoleAddDto, RoleModel>();
            CreateMap<RoleEditDto, RoleModel>();
            CreateMap<BlogModel, BlogAddDto>();
            CreateMap<BlogModel, BlogEditDto>();
            CreateMap<BlogAddDto, BlogModel>();
            CreateMap<BlogEditDto, BlogModel>();
            CreateMap<CommentModel, CommentAddDto>();
            CreateMap<CommentModel, CommentEditDto>();
            CreateMap<CommentAddDto, CommentModel>();
            CreateMap<CommentEditDto, CommentModel>();


        }

        
    }
}

// 1. download automapper nuGet package

// Assemblies are a collection of namespaces 
// 2. Services.AddAutoMapper(AppDomain CurrentDomain.GetAssemblies());

// 3. Create helper folder, which will store a file called AutoMapperProfiles.cs
// Inside this file, we create a class, which inheriting from Profile. 
// There are methods we want to use, which is called CreateMap from Profile, thus we inherit it 
// CreateMap< (source) , (destination) > ();
// 