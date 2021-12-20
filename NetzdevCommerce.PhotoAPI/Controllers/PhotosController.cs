using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetzdevCommerce.PhotoAPI.Models;

namespace NetzdevCommerce.PhotoAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PhotosController : ControllerBase
    {

        [HttpPost]
        public async Task<IActionResult> SavePhoto(IFormFile photo, CancellationToken cancellationToken)
        {
            if (photo != null && photo.Length > 0)
            {
                // Önce guid ile random bir dosya adı oluşturup uzantısını ekledik.
                var randomFileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);

                // Sonra tam ilgili konuma ilgili isimle yol oluşturduk.
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/photos", randomFileName);

                // İlgili yola bir stream açtık.
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    // Gelen fotoğrafı gönderdiğimiz yola ilgili isimle kopyala dedik.
                    await photo.CopyToAsync(stream, cancellationToken);
                }

                // Dönüş değeri olarak da yolu dönüyoruz.
                var returnPath = "photos/" + randomFileName;

                // Burada url parametresi olan boş bir class dönüyoruz.
                return Ok(new { Url = returnPath });
            }

            else
            {
                return BadRequest("Photo is null");
            }
        }

        [HttpDelete]
        public IActionResult DeletePhoto(DeletePhotoDto deletePhotoDto)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", deletePhotoDto.Url);

            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
                return NoContent();
            }
            return BadRequest();
        }


    }
}