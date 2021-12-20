using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NetzdevCommerce.Auth.Dtos;
using NetzdevCommerce.Auth.Models;
using NetzdevCommerce.Shared.Models;
using static IdentityServer4.IdentityServerConstants;

namespace NetzdevCommerce.Auth.Controllers
{
    // Burada api'yi api/controller/action olarak yapıyoruz. 
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        // Burada da userManager'ı Dependency Injection olarak alıyoruz.
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // Burada authorize attribute'ü ve parantez içinde belirttiğimiz scope ile sadece ilgili token gönderenlerin ulaşabileceği bir endpoint yaptık.
        [Authorize(LocalApi.PolicyName)]
        public IActionResult Test()
        {
            return Ok("test");
        }

        // Burası kullanıcı kaydetme methodu. Kullanıcı kaydı için önce http://localhost:5001/connect/token adresine AndroidClient_CC client id'si ve client secret ile istekte bulunuyoruz ve bir token alıyoruz. Sonra o token ile bu user controller'a istek atıp signup metodu ile kaydoluyoruz.
        [HttpPost]
        public async Task<IActionResult> SignUp(SignUpViewModel signUpViewModel)
        {
            // Bir application user nesnesi oluşturuyoruz ve bize gelen kayıt bilgilerini atıyoruz.
            var user = new ApplicationUser();

            user.UserName = signUpViewModel.UserName;
            user.Email = signUpViewModel.Email;
            user.City = signUpViewModel.City;

            // UserManager ile create metodunu çağırıyoruz ki ilgili bilgilerle kayıt yapsın ve sonuç dönüyor.
            var result = await _userManager.CreateAsync(user, signUpViewModel.Password);

            // Sonucun da başarılı olup olmadığını kontrol ediyoruz.
            if (!result.Succeeded)
            {
                var errorDto = new ErrorDto();
                errorDto.Status = 400;
                errorDto.IsShow = true;
                errorDto.Errors.AddRange(result.Errors.Select(x => x.Description).ToList());
                return BadRequest(errorDto);
            }

            return NoContent();
        }

        // Kullanıcıya ulaşıp token alma metodu. Mail ve şifre ile giriş yapıp token almak için http://localhost:5001/connect/token adresine AndroidClient_ROP olarak belirlediğimiz client id ve secret ile, mail adresi ve şifreyi gönderiyoruz ve bize ilgili kullanıcı için access token dönüyor.
        public async Task<IActionResult> GetUser()
        {
            // Burada bu metoda istek yaparken gönderdiğimiz token içindeki sub alanına ulaşıyoruz ve oradan userId'yi alıcaz.
            var userIdClaim = User.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub);

            // Claim'in null olup olmadığını kontrol ettik.
            if (userIdClaim == null) return BadRequest();

            // Claim içinden userId'yi aradık, böyle bir kullanıcı var mı diye kontrol ediyoruz.
            var user = await _userManager.FindByIdAsync(userIdClaim.Value);

            if (user == null) return BadRequest();

            // Bir userdto nesnesi oluşturduk ve onu dönüyoruz.
            var userdto = new ApplicationUserDto { UserName = user.UserName, Email = user.Email, City = user.City };

            return Ok(userdto);
        
        }

    }
}