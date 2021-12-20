using System;
using System.Collections.Generic;
using System.Text;

namespace NetzdevCommerce.Shared.Models
{
    public class ErrorDto
    {
        public ErrorDto()
        {
            Errors = new List<string>();
        }

        public List<string> Errors { get; set; }

        public int Status { get; set; }

        public bool IsShow { get; set; }
    }
}
