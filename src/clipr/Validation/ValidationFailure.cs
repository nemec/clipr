using System;

namespace clipr.Validation
{
    public class ValidationFailure : Exception
    {
        public ValidationFailure(string propertyName, string errorMessage)
            : this(propertyName, errorMessage, null)
        {
        }

        public ValidationFailure(string propertyName, string errorMessage, object attemptedValue)
            : base(String.Format("Property '{0}' failed to set value '{1}': {2}", propertyName, attemptedValue, errorMessage))
        {
            PropertyName = propertyName;
            ErrorMessage = errorMessage;
            AttemptedValue = attemptedValue;
        }

        public string PropertyName { get; private set; }

        public string ErrorMessage { get; private set; }

        public object AttemptedValue { get; private set; }
    }
}