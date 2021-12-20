using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetzdevCommerce.Product.API.Models;

namespace NetzdevCommerce.API.Controllers
{
    // Controller'ı OData'dan alıyoruz ve Authorize attribute'ü ile api'ye authentication eklediik.
    [Authorize]
    public class CategoriesController : ODataController
    {
        private readonly AppDbContext _context;

        public CategoriesController(AppDbContext context)
        {
            _context = context;
        }

        // Burada kategorileri ekledik, get metodu ekledik ve Kategorileri queryable olarak döndük ki OData filtreleme yapabilsin.
        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(_context.Categories.AsQueryable());
        }
    }
}