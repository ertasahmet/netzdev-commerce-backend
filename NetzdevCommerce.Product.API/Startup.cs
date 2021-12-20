using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation.AspNetCore;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetzdevCommerce.Product.API.Models;
using NetzdevCommerce.Shared.Extensions;
using NetzdevCommerce.Shared.Filters;

namespace NetzdevCommerce.Product.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });

            // Burada diyoruz ki bu projeye �nce authentication kullanaca��m�z� ekle, sonra bu authentication i�lemini JwtBearer ile yapaca��m�z� belirle ve �emam�z� da ikisini ayn� yap ki haberle�ebilsinler. Optionslar�na da diyoruz ki bu token'lar� localhost:5001 adresinden do�rula diyoruz ��nk� identityServer orada �al���yor. Buray� ger�ek hayatta identity.netzdev.com gibi bir domaine at�p adresin oldu�u yeri de appsettings'ten falan �ekebiliriz. Audience k�sm� da hangi auth projesinde olu�turdu�umuz apiResource k�s�mlar�nda tan�mlad���m�z keydir. Bu key ile kendini tan�t diyoruz. sorna da https olay�n� false yap diyoruz, https olucaksa true yapar�z.
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Authority = "http://localhost:5001";
                options.Audience = "resource_product_api";
                options.RequireHttpsMetadata = false;
            });

            // Burada da OData'y� ekledik.
            services.AddOData();

            // Burada da controller'lar� ekleyip option olarak filter ekledik, bu filter da bizim yazd���m�z modelState'leri bizim hatalar�m�z t�r�nde d�nen tipti. Daha sonra fluent validation ekledik.
            services.AddControllers(options =>
            {
                options.Filters.Add<ValidateModelAttribute>();

            }).AddFluentValidation(options =>
            {
                options.RegisterValidatorsFromAssemblyContaining<Startup>();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                // Sadece development modundayken bu delay metodunu �al��t�r diyoruz.
                app.UseDelayRequestDevelopment();
            }

            // Burada da custom olarka olu�turdu�umuz exception ayarlar�n� kullan�yoruz.
            app.UseCustomException();

            app.UseRouting();

            // Buraya da use Authentication diyoruz.
            app.UseAuthentication();
            app.UseAuthorization();

            // Burada da OData'dan bir model builder tan�mlad�k. �lgili modelleri ve Controller'lar� ekledik. Generic olanlar model, parantez i�inde yazanlar ise olu�turaca��m�z Controller'lar�n ismidir.
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Models.Product>("Products");
            builder.EntitySet<Category>("Categories");

            app.UseEndpoints(endpoints =>
            {
                // Burada url'lerde hangi i�lemleri kullanaca��m�z� s�yl�yoruz. Mesela select i�lemi istedi�imiz propertyleri almam�z� sa�l�yor, expand ile foreignKey'li olan s�n�flar� yani kategorileri alabiliriz. ORderBy ile s�ralama, count ile �r�n say�s�, filter ile de arama gibi �eyleri yapabiliriz o y�zden ekledik.
                endpoints.Select().Expand().OrderBy().Count().Filter();

                // Burada ise ilk k�s�mda odata isimli bir route tan�mlad�k. �kinci parametre url'de odata kullan dedik yani url -> odata/products �eklinde olucak. Son parametre de biz tan�mlad���m�z bu entitylerle hangi �zelliklere ve modellere ula�abiliriz onu g�stersin diyoruz.
                endpoints.MapODataRoute("odata", "odata", builder.GetEdmModel());

                endpoints.MapControllers();
            });
        }
    }
}
