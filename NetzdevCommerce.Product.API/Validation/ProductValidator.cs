using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetzdevCommerce.API.Validation
{
    public class ProductValidator : AbstractValidator<Product.API.Models.Product>
    {
        public ProductValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Ürün ismi gereklidir.");
            RuleFor(x => x.Stock).NotEmpty().WithMessage("Stok alanı gereklidir.");
            RuleFor(x => x.Price).NotEmpty().WithMessage("Ürün fiyatı gereklidir.");
            RuleFor(x => x.Color).NotEmpty().WithMessage("Ürün rengi gereklidir.");
        }
    }
}
