using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using MyCodeCamp.Data.Entities;

namespace MyCodeCamp.Models
{
    public class CampMappingProfile : Profile
    {
        public CampMappingProfile()
        {
            CreateMap<Camp,CampModel>()
                .ForMember(c => c.StartDate, opt => opt.MapFrom(camp => camp.EventDate))
                .ForMember(c => c.EndDate, opt => opt.ResolveUsing(camp => camp.EventDate.AddDays(camp.Length - 1)))
                .ForMember(c => c.Url,opt => opt.ResolveUsing<CampUrlResolver>())
                .ReverseMap()
                .ForMember(m => m.EventDate, opt => opt.MapFrom(model => model.StartDate))
                .ForMember(m => m.Length, opt => opt.ResolveUsing(model => (model.EndDate - model.StartDate).Days + 1))
                .ForMember(m => m.Location, opt => opt.ResolveUsing(model => new Location()
                {
                    Address1 = model.LocationAddress1,
                    Address2 = model.LocationAddress2,
                    Address3 = model.LocationAddress3,
                    CityTown = model.LocationCityTown,
                    StateProvince = model.LocationStateProvince,
                    PostalCode = model.LocationPostalCode,
                    Country = model.LocationCountry
                }));

            CreateMap<Speaker,SpeakerModel>()
                .ForMember(s => s.Url, opt => opt.ResolveUsing<SpeakerUrlResolver>())
                .ReverseMap();

            CreateMap<Speaker,SpeakerV2Model>()
                .IncludeBase<Speaker,SpeakerModel>()
                .ForMember(s => s.BadgeName,opt => opt.ResolveUsing(s => $"{s.Name} (@{s.TwitterName})"));

            CreateMap<Talk,TalkModel>()
                .ForMember(t => t.Url, opt => opt.ResolveUsing<TalkUrlResolver>())
                .ForMember(t => t.Links, opt => opt.ResolveUsing<TalkLinksResolver>())
                .ReverseMap();
        }
    }
}
