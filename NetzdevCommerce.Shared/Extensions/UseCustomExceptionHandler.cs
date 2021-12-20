using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using NetzdevCommerce.Shared.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetzdevCommerce.Shared.Extensions
{
    // Burada özel bir exception handler yazdık burada hataları ele alıyoruz.
    public static class UseCustomExceptionHandler
    {
        // Parametre olarak Startup'da app.Use bişeyler diye kullandığımız ApplicationBuilder alıyoruz çünkü hata olayında araya girip burayı çalıştırıcaz.
        public static void UseCustomException(this IApplicationBuilder app)
        {
            // App'teki exception handler'ı kullanıp araya giriyoruz. Config ile içeri girdik ve run dedik.
            app.UseExceptionHandler(config =>
            {
                // Burada da context'i lambda ile içeri alıyoruz.
                config.Run(async context =>
                {
                    // Burada context'in responslarında status code'unu 500 yapıyoruz çünkü server tarafında hata oluşmuştur ve content type'ı json yapıyoruz çünkü json dönücez.
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "application/json";

                    // Context'in içinden erroru alıyoruz.
                    var error = context.Features.Get<IExceptionHandlerFeature>();

                    if (error != null)
                    {
                        // Erro'un içindeki error'u aldık.
                        var ex = error.Error;

                        // ErrorDto nesnemizi oluşturup error'u oraya ekliyoruz.
                        ErrorDto errorDto = new ErrorDto();
                        errorDto.Status = 500;
                        errorDto.Errors.Add(ex.Message);

                        // Burada da diyoruz ki eğer hata nesnesinin türü bizim CustomException sınıfımızın türünden ise kullanıcıya göstermek için isShow'u true yap, normal hataysa gösterme
                        if (ex is CustomException)
                        {
                            errorDto.IsShow = true;
                        }
                        else
                        {
                            errorDto.IsShow = false;
                        }

                        // Burada da context'te Response olarak errorDto nesnemizi json'a çevirip dön diyoruz.
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(errorDto));
                    }
                });
            });
        }

    }
}
