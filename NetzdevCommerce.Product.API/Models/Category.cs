using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetzdevCommerce.Product.API.Models
{
    // Bir üründe birden fazla product olacağı için ICollection şeklinde tanımlıyoruz.
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<Product> Products { get; set; }
    }
}
