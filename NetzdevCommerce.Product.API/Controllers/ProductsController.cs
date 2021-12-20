using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetzdevCommerce.Product.API;
using NetzdevCommerce.Product.API.Models;

namespace NetzdevCommerce.API.Controllers
{
    [Authorize]
    public class ProductsController : ODataController
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // Burada OData'nın otomatik isimlendirmesine uyuyoruz. Metodlara farklı isimler koyarsak odata'yı ayarlamamız gerekiyor.
        // Burada url -> odata/products şeklindedir. Bu endpoint'e de query yapılabilmesi için EnableQuery yaptık.
        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(_context.Products.AsQueryable());
        }

        // Burada da sadece tek bir ürün dönmek için metod tanımladık. Parametre olarak OData'nın Url'inden key isimli id alıyor. Burada dikkat edilmesi gereken şey her metodun IQueryable tipinde dönmesidir. Sebebi de OData ona göre konfigürasyon yapıyor. Burada url de şöyle oluyor -> odata/products(2)
        [EnableQuery]
        public IActionResult Get([FromODataUri] int key)
        {
            return Ok(_context.Products.Where(x => x.Id == key));
        }

        // Burada bir ürün ekliyoruz. Body'den bir product nesnesi alıyoruz ve ekliyoruz.
        [HttpPost]
        public async Task<IActionResult> PostProduct([FromBody] Product.API.Models.Product product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            return Ok(product);
        }

        // Ürün güncellenmesi için de ODataUri den ürün id'sini alıyoruz ve bir de yeni product'ı alıyoruz.
        [HttpPut]
        public async Task<IActionResult> PutProduct([FromODataUri] int key, [FromBody] Product.API.Models.Product product)
        {
            // Gönderilen id'yi product'ın id'sine atıyoruz. 
            product.Id = key;

            // Product'ın state'ini güncelleme olarak değiştirdik.
            _context.Entry(product).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

            // Burada da db'ye ekledik.
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // Delete metodu oluşturduk ve ODataUri den silinecek ürünün id'sini aldık.
        [HttpDelete]
        public async Task<IActionResult> DeleteProduct([FromODataUri] int key)
        {
            // Önce ürünü bulduk, ürün null ise bulunamadı dön.
            var product = await _context.Products.FindAsync(key);
            if (product == null) return NotFound();

            // Ürünü sildik ve değişiklikleri kaydettik.
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}