﻿using Business;
using Business.Interfaces;
using Entities.DTO.Request.Day;
using Entities.DTO.Request.Person;
using Entities.DTO.Request.Schedule;
using Entities.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Interfaces;
using Persistence.Interfaces.GenericRepository;
using Persistence.Repository;
using Persistence.Repository.GenericRepository;

namespace API.DependencyInjection
{
    public static class NativeInjectorConfig
    {
        public static void RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            // identity configuration
            services.AddDefaultIdentity<IdentityUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApiContext>()
                .AddDefaultTokenProviders();

            services.AddScoped<IdentityDbContext, ApiContext>();

            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IDayRepository, DayRepository>();
            services.AddScoped<IPersonRepository, PersonRepository>();
            services.AddScoped<IDayPersonRepository, DayPersonRepository>();
            services.AddScoped<IScheduleRepository, ScheduleRepository>();

            services.AddScoped<IBusiness<PersonDtoRequest, PersonEntity>, PersonBusiness>();
            services.AddScoped<IBusiness<DayDtoRequest, DayEntity>, DayBusiness>();
            services.AddScoped<IBusiness<ScheduleDtoRequest, ScheduleEntity>, ScheduleBusiness>();
            services.AddScoped<IIdentityBusiness, IdentityBusiness>();
            services.AddScoped<IJwtBusiness, JwtBusiness>();
        }
    }
}
