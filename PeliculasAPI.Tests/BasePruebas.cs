﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WebApiPeliculasFinal;
using WebApiPeliculasFinal.Helpers;

namespace PeliculasAPI.Tests
{
    public class BasePruebas
    {
        protected string usuarioPorDefectoId = "0010a997-8934-448e-be55-611f3c98791a";
        protected string usuarioPorDefectoEmail = "usuarionormal@hotmail.com";


        //Cada prueba unitaria creará su propia base de datos
        protected ApplicationDbContext ConstruirContext(string nombreDB)
        {
            var opciones = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(nombreDB).Options;

            var dbContext = new ApplicationDbContext(opciones);

            return dbContext;

        }

        protected IMapper ConfigurarAutoMapper()
        {
            var config = new MapperConfiguration(options =>
            {
                var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
                options.AddProfile(new AutoMapperProfiles(geometryFactory));
            });

            return config.CreateMapper();
        }

        protected ControllerContext ConstruirControllerContext()
        {
            var usuario = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, usuarioPorDefectoEmail),
                new Claim(ClaimTypes.Email, usuarioPorDefectoEmail),
                new Claim(ClaimTypes.NameIdentifier, usuarioPorDefectoId)
            }));

            return new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = usuario }
            };
        }

        protected WebApplicationFactory<Startup> ConstruirWebApplicationFactory(string nombreBD,
            bool ignorarSeguridad = true) //Evitar el atributo Authorize
        {
            var factory = new WebApplicationFactory<Startup>();

            factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    var descriptorDBContext = services.SingleOrDefault(d =>
                    d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                    if (descriptorDBContext != null)
                    {
                        services.Remove(descriptorDBContext);
                    }

                    services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase(nombreBD));

                    if (ignorarSeguridad)
                    {
                        services.AddSingleton<IAuthorizationHandler, AllowAnonymousHandler>();

                        services.AddControllers(options =>
                        {
                            options.Filters.Add(new UsuarioFalsoFiltro());
                        });
                    }
                });
            });

            return factory;
        }
    }
}