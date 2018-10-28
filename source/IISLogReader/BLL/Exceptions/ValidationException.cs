using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.BLL.Exceptions
{
    public class ValidationException : Exception
    {
        public ValidationException(string validationError) : this(new string[] {  validationError })
        {
        }

        public ValidationException(IEnumerable<string> validationErrors) : base(String.Join(",", validationErrors))
        {
            this.ValidationErrors = validationErrors.ToArray();
        }

        public string[] ValidationErrors { get; set; }
    }
}
