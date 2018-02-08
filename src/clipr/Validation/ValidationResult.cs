
using System.Collections.Generic;
using System.Linq;

namespace clipr.Validation
{
    public class ValidationResult
    {
        public IList<ValidationFailure> Errors { get; private set; }

        public bool IsValid
        {
            get
            {
                return Errors.Count == 0;
            }
        }

        public ValidationResult()
        {
            Errors = new List<ValidationFailure>();
        }

        public ValidationResult(IEnumerable<ValidationFailure> errors)
        {
            Errors = errors.Where(e => e != null).ToList();
        }
    }
}