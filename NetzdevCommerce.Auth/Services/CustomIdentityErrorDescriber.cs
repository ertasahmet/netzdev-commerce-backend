using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetzdevCommerce.Auth.Services
{
    public class CustomIdentityErrorDescriber : IdentityErrorDescriber
    {
        // Services klasörü içine bir class açtık ve IdentityErrorDescriber class'ından miras aldık. Hata mesajlarını override ettik ve türkçe yapıp geri döndürdük.
        public override IdentityError DuplicateEmail(string email)
        {
            return new IdentityError() { Code = "DuplicateEmail", Description = $"'{email}' adresi zaten mevcut. Lütfen farklı bir mail adresi deneyin." };
        }

        public override IdentityError DuplicateUserName(string userName)
        {
            return new IdentityError() { Code = "DuplicateEmail", Description = $"'{userName}' kullanıcı adı zaten mevcut. Lütfen farklı bir kullanıcı adı deneyin." };
        }

    }
}
