using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace NetzdevCommerce.Shared.Models
{
    // Hataların bizim bilerek kullanıcıya göstermek için oluşturduğumuz şeyler mi yoksa normal hatalar mı olduğunu ayırmak için böyle bir class oluşturduk ve metodları implemente ettik. Eğer Hatalar CustomException türünden olursa bizim kullanıcıya göstereceğimiz hatalardır.
    public class CustomException : Exception
    {
        public CustomException()
        {
        }

        public CustomException(string message) : base(message)
        {
        }

        public CustomException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CustomException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
