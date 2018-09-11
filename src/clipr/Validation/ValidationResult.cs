
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

        /// <summary>
        /// A successful validation result
        /// </summary>
        public ValidationResult()
        {
            Errors = new List<ValidationFailure>();
        }

        /// <summary>
        /// A validation result with a list of failures. If list is
        /// empty or contains only null value, the validation is
        /// considered successful.
        /// </summary>
        /// <param name="errors"></param>
        public ValidationResult(IEnumerable<ValidationFailure> errors)
        {
            Errors = errors.Where(e => e != null).ToList();
        }
    }
}