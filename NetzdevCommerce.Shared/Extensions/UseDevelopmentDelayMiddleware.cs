using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NetzdevCommerce.Shared.Extensions
{
    public static class UseDevelopmentDelayMiddleware
    {
        // Burada bir metod tanımladık ve gelen istekler gerçekleşmeden bi araya giriyoruz, isteği geciktiriyoruz ve sonra Invoke metoduyla devam et diyoruz.
        public static void UseDelayRequestDevelopment(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                await Task.Delay(1000);
                await next.Invoke();
            });
        }

    }
}
