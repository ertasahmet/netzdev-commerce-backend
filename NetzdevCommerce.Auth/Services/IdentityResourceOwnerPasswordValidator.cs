using IdentityModel;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Identity;
using NetzdevCommerce.Auth.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetzdevCommerce.Auth.Services
{

    public class IdentityResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public IdentityResourceOwnerPasswordValidator(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            var existUser = await _userManager.FindByEmailAsync(context.UserName);

            // Burada eğer ilgili bilgilerle kullanıcı yoksa hata vericez.
            if (existUser == null)
            {
                var errors = new Dictionary<string, object>();

                // Önce bir dictionary oluşturduk ve bizim errorDto nesnemiz gibi fieldlar açtık ve bilgilerini yazdık.
                errors.Add("errors", new List<string> { "Email veya şifreniz yanlış." });
                errors.Add("status", 400);
                errors.Add("isShow", true);

                // Custom response alanına da bu alanlarımızı ekledik ki hata olunca bunları json şeklinde dönecek ve biz de android tarafında yakalayacaz.
                context.Result.CustomResponse = errors;
                return;
            }

            var passwordCheck = await _userManager.CheckPasswordAsync(existUser, context.Password);

            if (passwordCheck == false)
            {
                var errors = new Dictionary<string, object>();

                errors.Add("errors", new List<string> { "Email veya şifreniz yanlış." });
                errors.Add("status", 400);
                errors.Add("isShow", true);

                context.Result.CustomResponse = errors;
                return;
            }

            // Burada kullandığımız bu grant type'lardan ResourceOwnerPasswordValidation 'ın olayı password doğrulaması ile kontrol ediyor. O doğrulama şeklini kullandığımızı söylüyoruz. Burada da onu OidcConstants.AuthenticationMethods.Password şeklinde belirttik.
            context.Result = new GrantValidationResult(existUser.Id.ToString(), OidcConstants.AuthenticationMethods.Password);
        }

    }
}
