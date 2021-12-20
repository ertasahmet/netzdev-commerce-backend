// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4;
using IdentityServer4.Models;
using System;
using System.Collections.Generic;

namespace NetzdevCommerce.Auth
{
    public static class Config
    {
        // En başta IdentityServer4'ün github sayfasına girdik. Orada installation kısmındaki dotnet new -i identityserver4.templates kodunu powershell açıp oraya yazıcaz. Daha sonra solution'ın olduğu dizinde powershell açıyoruz ve oraya dotnet new is4aspid -n NetzdevCommerce.Auth şeklinde proje adı yazıyoruz en sona ve bu şekilde auth projesi oluşmuş oluyor. 

        // Sonra startup'a gelip UseSqlite olan kısmı UseSqlServer yapıyoruz, Nuget'ten EntityFrameworkCore ve Tools kuruyoruz. Appsettings'te connection string'i değiştiriyoruz.

        // ApplicationUser modelinde ekstra istediğimiz alanlar varsa ekliyoruz. Projede hazır olan migration klasörünü siliyoruz ve package manager console açıyoruz. Oraya add-migration Initial yazıyoruz ve yeni migration dosyası oluşturuyor. Sonra update-database diyoruz ve tablolar veritabanına oluşturuluyor.

        // Config dosyasında ApiResources ile hangi apilerden bu identity apisine istekte bulunacağımızı söylüyoruz ve bunların scope'larını belirtiyoruz. Aynı zamanda en son eklediğimiz de bu NetzdevCommerce.Auth projesinde de kullanıcının kaydolabileceği vs. işlemleri yazan bir api yazıcağımız için onu da client olarak ekledik.

        // Burada oluşturduğumuz ApiSecret yazan yerde bu Api Resource için bir şifre belirliyoruz ve postman'de authorization kısmında basic auth seçip kaynak kısmına resource_product_api, şifre kısmına da apiSecret kısmına yazdığımız secret'ı gireriz. FormUrlEncoded kısmına da token keyword'u ile elimizdeki access token'ı /connect/introspect adresine istekte bulunursak bize elimizdeki token'ın geçerli mi, değil mi olduğunu dönecek. Bu yüzden bu apiSecret kısmını ekliyoruz.
        public static IEnumerable<ApiResource> apiResources => new ApiResource[]
        {
            new ApiResource("resource_product_api"){
                Scopes = {"api_product_fullpermission"},
                ApiSecrets = new [] {new Secret("apisecret".Sha256())} },

            new ApiResource("resource_photo_api"){
                Scopes = {"api_photo_fullpermission"},
                ApiSecrets = new [] {new Secret("apisecret".Sha256())} },

            new ApiResource(IdentityServerConstants.LocalApi.ScopeName)
        };

        // Buradaki scope'larda da ilgili scope'ları tanımlıyoruz ve açıklama yazıyoruz. En sondaki scope da IdentityServerApi yazmak ile aynı işlemi görüyor. Burada amaç identity işlemleri için yazacağımız api için bir scope oluşturmak.
        public static IEnumerable<ApiScope> ApiScopes =>
          new ApiScope[]
          {
                new ApiScope("api_product_fullpermission", "Product API için tüm izinler"),
                new ApiScope("api_photo_fullpermission", "Photo API için tüm izinler"),
                new ApiScope(IdentityServerConstants.LocalApi.ScopeName)
          };

        // IdentityResources kısmında ise token bize döndüğünde içinde hangi bilgiler olsun onu söylüyoruz, aşağıdaki ikisi vardı, biz ekstra email de dönsün dedik ve onu ekledik.
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
                new IdentityResources.Email(),
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };


        // Bu clients kısmında apilere istek atabilecek client'lari tanımlıyoruz.   
        public static IEnumerable<Client> Clients =>
            new Client[]
            {
                
                // Buradaki client android içinden identityServer'a ulaşması için tanımladığımız client'tır. İlgili client id ve client name ile sadece identityServer Api'sine token almak, üye giriş ve kaydı yapmak için kullanılır. Diğer Product ve Photo apilerine bu client id ile istek atamaz.
                new Client
                {
                    ClientId = "AndroidClient_CC",
                    ClientName = "AndroidClient CC",

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets = { new Secret("secret".Sha256()) },

                    AllowedScopes = { IdentityServerConstants.LocalApi.ScopeName }
                },

                // interactive client using code flow + pkce
                new Client
                {
                    ClientId = "AndroidClient_ROP",
                      ClientName = "AndroidClient ROP",
                    ClientSecrets = { new Secret("secret".Sha256()) },

                    // GrantType muhabbeti de burada ResourceOwnerPassword yazarsak buraya istek atan kişi hesabın sahibi olduğunu emailini ve şifresini vs girerek doğrulamak zorunda diyoruz.
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,

                    // Offline Access arkaplanda token ömrü dolduğunda kullanıcının haberi olmadan refresh token ile yeni token alma olayıdır.
                    AllowOfflineAccess = true,

                    // Burada da diyoruz ki bu client id ile istek atarken hangi izne sahip olduğunu söylüyoruz. Bu tanımladığımız verilere bu client erişebilecek diyoruz. Ve diğer product ve photo api için de izin veriyoruz ki oraya da istek atabilsin. OfflineAccess ile refresh token alması için de izin veriyoruz. 
                    AllowedScopes = { IdentityServerConstants.StandardScopes.Email, IdentityServerConstants.StandardScopes.OpenId, IdentityServerConstants.StandardScopes.Profile,
                      "api_product_fullpermission", "api_photo_fullpermission", IdentityServerConstants.StandardScopes.OfflineAccess},

                    // Token ömrünü 10 dakika belirledik.
                    AccessTokenLifetime = 10*60,

                    // Bu da refresh token'ı ömrü içinde sürekli olarak kullanabilmemizi sağlıyor. Reuse olmasaydı sadece 1 kere ömrü içinde kullanabilecektik.
                    RefreshTokenUsage = TokenUsage.ReUse,

                    // Eğer bu sliding olursa bir refresh token ile tokenı yenilediğimizde refresh token süresini 2 ay belirlediysek refresh token'ın expiration tarihi 2 ay daha uzatılır, aldıkça uzatılır. Eğer Absolute olursa kullanıcı ilk oturumu açtıktan 2 aylık süre verir, kesin bir tarihe işaret eder ve o tarihte refresh token süresi biter.
                    RefreshTokenExpiration = TokenExpiration.Sliding,

                    // 60 Günlük süre belirledik, refresh token için.
                    SlidingRefreshTokenLifetime = (int) (DateTime.Now.AddDays(60)-DateTime.Now).TotalSeconds
                    

                },
            };
    }
}