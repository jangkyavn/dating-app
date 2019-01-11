﻿using AutoMapper;
using DatingApp.API.Dtos;
using DatingApp.Data.Entities;
using System.Linq;

namespace DatingApp.API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<User, UserForListDto>()
                .ForMember(dest => dest.PhotoUrl, opt =>
                {
                    opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url);
                })
                .ForMember(dest => dest.Age, opt =>
                {
                    opt.MapFrom(d => d.DateOfBirth.CalculateAge());
                });
            CreateMap<User, UserForDetailedDto>()
                .ForMember(dest => dest.PhotoUrl, opt =>
                {
                    opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url);
                })
                .ForMember(dest => dest.Age, opt =>
                {
                    opt.MapFrom(d => d.DateOfBirth.CalculateAge());
                });
            CreateMap<Photo, PhotoForDetailedDto>();
            CreateMap<Photo, PhotoForReturnDto>();
            CreateMap<Message, MessageToReturnDto>()
                .ForMember(m => m.SenderPhotoUrl, opt => {
                    opt.MapFrom(u => u.Sender.Photos.FirstOrDefault(p => p.IsMain).Url);
                })
                .ForMember(m => m.RecipientPhotoUrl, opt => {
                    opt.MapFrom(u => u.Recipient.Photos.FirstOrDefault(p => p.IsMain).Url);
                });

            CreateMap<UserForUpdateDto, User>();
            CreateMap<PhotoForCreationDto, Photo>();
            CreateMap<UserForRegisterDto, User>();
            CreateMap<MessageFromCreationDto, Message>().ReverseMap();
        }
    }
}
