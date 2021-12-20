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

            // Burada diyoruz ki bu projeye önce authentication kullanacaðýmýzý ekle, sonra bu authentication iþlemini JwtBearer ile yapacaðýmýzý belirle ve þemamýzý da ikisini ayný yap ki haberleþebilsinler. Optionslarýna da diyoruz ki bu token'larý localhost:5001 adresinden doðrula diyoruz çünkü identityServer orada çalýþýyor. Burayý gerçek hayatta identity.netzdev.com gibi bir domaine atýp adresin olduðu yeri de appsettings'ten falan çekebiliriz. Audience kýsmý da hangi auth projesinde oluþturduðumuz apiResource kýsýmlarýnda tanýmladýðýmýz keydir. Bu key ile kendini tanýt diyoruz. sorna da https olayýný false yap diyoruz, https olucaksa true yaparýz.
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Authority = "http://localhost:5001";
                options.Audience = "resource_product_api";
                options.RequireHttpsMetadata = false;
            });

            // Burada da OData'yý ekledik.
            services.AddOData();

            // Burada da controller'larý ekleyip option olarak filter ekledik, bu filter da bizim yazdýðýmýz modelState'leri bizim hatalarýmýz türünde dönen tipti. Daha sonra fluent validation ekledik.
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

                // Sadece development modundayken bu delay metodunu çalýþtýr diyoruz.
                app.UseDelayRequestDevelopment();
            }

            // Burada da custom olarka oluþturduðumuz exception ayarlarýný kullanýyoruz.
            app.UseCustomException();

            app.UseRouting();

            // Buraya da use Authentication diyoruz.
            app.UseAuthentication();
            app.UseAuthorization();

            // Burada da OData'dan bir model builder tanýmladýk. Ýlgili modelleri ve Controller'larý ekledik. Generic olanlar model, parantez içinde yazanlar ise oluþturacaðýmýz Controller'larýn ismidir.
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Models.Product>("Products");
            builder.EntitySet<Category>("Categories");

            app.UseEndpoints(endpoints =>
            {
                // Burada url'lerde hangi iþlemleri kullanacaðýmýzý söylüyoruz. Mesela select iþlemi istediðimiz propertyleri almamýzý saðlýyor, expand ile foreignKey'li olan sýnýflarý yani kategorileri alabiliriz. ORderBy ile sýralama, count ile ürün sayýsý, filter ile de arama gibi þeyleri yapabiliriz o yüzden ekledik.
                endpoints.Select().Expand().OrderBy().Count().Filter();

                // Burada ise ilk kýsýmda odata isimli bir route tanýmladýk. Ýkinci parametre url'de odata kullan dedik yani url -> odata/products þeklinde olucak. Son parametre de biz tanýmladýðýmýz bu entitylerle hangi özelliklere ve modellere ulaþabiliriz onu göstersin diyoruz.
                endpoints.MapODataRoute("odata", "odata", builder.GetEdmModel());

                endpoints.MapControllers();
            });
        }
    }
}
