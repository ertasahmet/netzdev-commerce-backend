using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NetzdevCommerce.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetzdevCommerce.Shared.Filters
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        // Bu metod fluent validation çalıştığında validation işlemi yapıldıktan sonra OData araya girmeden araya girmek için kullanılıyor. Bunun sebebi FluentValidation OData ile direk çalışamıyor o yüzden bu şekilde bir filter yazıyoruz.
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // ModelState'i kontrol ettik.
            if (context.ModelState.IsValid == false)
            {
                var errors = context.ModelState.Values.Where(x => x.Errors.Count > 0).SelectMany(x => x.Errors).Select(x => x.ErrorMessage);

                // ErrorDto nesnesini oluşturuyoruz ve elimizdeki listeyi errors listemize direk ekliyoruz.
                ErrorDto errorDto = new ErrorDto();
                errorDto.Errors.AddRange(errors);
                errorDto.Status = 400;
                errorDto.IsShow = true;

                // En son da BadRequestObject diyerek errorDto'yu obje yani json olarak result'a  atıyoruz.
                context.Result = new BadRequestObjectResult(errorDto);
            }
        }

    }
}
