using AutoMapper;
using WorkingShiftActivity.Context.Models;
using WorkingShiftActivity.Models.ResponseModels;

namespace WorkingShiftActivity.Helper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Activity, ActivityResponse>()
                .ReverseMap();
        }
    }
}
