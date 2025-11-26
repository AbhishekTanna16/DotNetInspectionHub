using AutoMapper;
using ShopInspector.Core.Entities;
using ShopInspector.Web.Areas.Admin.Models;

namespace ShopInspector.Web.Mappings;

public class CompanyMappingProfile : Profile
{
    public CompanyMappingProfile()
    {
        // Company mappings
        CreateMap<Company, CompanyListViewModel>()
            .ForMember(dest => dest.EmployeesCount, opt => opt.MapFrom(src => src.Employees != null ? src.Employees.Count : 0));
        
        CreateMap<Company, CompanyEditViewModel>();
        
        CreateMap<CompanyCreateViewModel, Company>()
            .ForMember(dest => dest.CompanyID, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Employees, opt => opt.Ignore());

        CreateMap<CompanyEditViewModel, Company>()
            .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Employees, opt => opt.Ignore());

        // Employee mappings
        CreateMap<Employee, EmployeeListViewModel>()
            .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company != null ? src.Company.CompanyName : string.Empty));
        
        CreateMap<Employee, EmployeeEditViewModel>();
        
        CreateMap<EmployeeCreateViewModel, Employee>()
            .ForMember(dest => dest.EmployeeID, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Company, opt => opt.Ignore());

        CreateMap<EmployeeEditViewModel, Employee>()
            .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Company, opt => opt.Ignore());

        // InspectionFrequency mappings
        CreateMap<InspectionFrequency, InspectionFrequencyListViewModel>()
            .ForMember(dest => dest.InspectionsCount, opt => opt.MapFrom(src => src.AssetInspections != null ? src.AssetInspections.Count : 0));
        
        CreateMap<InspectionFrequency, InspectionFrequencyEditViewModel>();
        
        CreateMap<InspectionFrequencyCreateViewModel, InspectionFrequency>()
            .ForMember(dest => dest.InspectionFrequencyID, opt => opt.Ignore())
            .ForMember(dest => dest.AssetInspections, opt => opt.Ignore());

        CreateMap<InspectionFrequencyEditViewModel, InspectionFrequency>()
            .ForMember(dest => dest.AssetInspections, opt => opt.Ignore());
    }
}