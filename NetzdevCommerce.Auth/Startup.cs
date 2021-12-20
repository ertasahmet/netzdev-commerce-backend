// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4;
using NetzdevCommerce.Auth.Data;
using NetzdevCommerce.Auth.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetzdevCommerce.Auth.Services;
using FluentValidation.AspNetCore;
using NetzdevCommerce.Shared.Extensions;

namespace NetzdevCommerce.Auth
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Burada da bu proje aynı zamanda kullanıcı girişi işlemleri yapıp api olacağı için bu metodu ekliyoruz.
            services.AddLocalApiAuthentication();

            // Burada mvc'yi eklerken yanına fluent validation'ı ekledik, ve içine de Startup Class'ını içeren tüm validation'ları al ve işle diyoruz.
            services.AddControllersWithViews().AddFluentValidation(options =>
            {
                options.RegisterValidatorsFromAssemblyContaining<Startup>();
            });


            //Burada FluentValidation'u implemente ettikten sonra ekliyoruz middleware'i.
            services.UseCustomValidationResponse();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // Burada identity'i eklerken options ile içine giriyoruz ve bazı şeyler için özelleştirme yapıyoruz.
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {

                // Unique bir email olsun diyoruz, şifrede alfa numerik karakter ve büyük harf zorunluluğu olmasın diyoruz.
                options.User.RequireUniqueEmail = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;

            })      
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()

                // Burada da hataları türkçe yaptığımız class'ı burada ErrorDescriber olarak ekledik.
                .AddErrorDescriber<CustomIdentityErrorDescriber>();

            var builder = services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
                options.IssuerUri = "http://localhost:5001";
                // see https://identityserver4.readthedocs.io/en/latest/topics/resources.html
                options.EmitStaticAudienceClaim = true;
            })

                // Burada identity resource ve api scope'lar falan vardı fakat api Resource yoktu 2. satırda onu ekledik.
                .AddInMemoryIdentityResources(Config.IdentityResources)
                .AddInMemoryApiResources(Config.apiResources)
                .AddInMemoryApiScopes(Config.ApiScopes)
                .AddInMemoryClients(Config.Clients)
                .AddAspNetIdentity<ApplicationUser>();

            // Burada da ekstra AddResourceOwnerValidator diyerek kendi password validator sınıfımızı ekledik.
            builder.AddDeveloperSigningCredential().AddResourceOwnerValidator<IdentityResourceOwnerPasswordValidator>();
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseDelayRequestDevelopment();
            }

            app.UseCustomException();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseIdentityServer();

            // Buraya da authetication ekliyoruz ki tokenlar kontrol edilsin.
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}