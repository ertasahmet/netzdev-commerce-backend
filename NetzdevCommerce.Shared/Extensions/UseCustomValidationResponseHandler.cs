using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using NetzdevCommerce.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetzdevCommerce.Shared.Extensions
{
    public static class UseCustomValidationResponseHandler
    {
        // Burada validation işlemi için middleware yazdık. Parametre olarak startup.cs 'teki ConfigureServices metodundaki services'i aldık.
        public static void UseCustomValidationResponse(this IServiceCollection services)
        {
            // Api davranışlarına göre configure et diyoruz.
            services.Configure<ApiBehaviorOptions>(options =>
            {
                // Geçersiz bir model state geldiğinde diyoruz.
                options.InvalidModelStateResponseFactory = context =>
                {
                    // Context'teki modelState'den gelen verilerde Error'ların sayısı 0 dan büyükse yani varsa errorları Select Many ile al, her bir error'un da Error Message'ını al ve errors listesine ata diyoruz.
                    var errors = context.ModelState.Values.Where(x => x.Errors.Count > 0).SelectMany(x => x.Errors).Select(x => x.ErrorMessage);

                    // ErrorDto nesnesini oluşturuyoruz ve elimizdeki listeyi errors listemize direk ekliyoruz.
                    ErrorDto errorDto = new ErrorDto();
                    errorDto.Errors.AddRange(errors);
                    errorDto.Status = 400;
                    errorDto.IsShow = true;

                    // En son da BadRequestObject diyerek errorDto'yu obje yani json olarak dönüyoruz.
                    return new BadRequestObjectResult(errorDto);
                };
            });
        }

    }
}
