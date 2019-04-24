using System;

namespace clipr.Validation
{
    public class ValidationFailure : Exception
    {
        public ValidationFailure(string propertyName, string errorMessage)
            : base(String.Format("Property '{0}' failed to set value: {1}", propertyName, errorMessage))
        {
            PropertyName = propertyName;
            ErrorMessage = errorMessage;
        }

        public ValidationFailure(string propertyName, string errorMessage, object attemptedValue)
            : base(FormatMessage(propertyName, errorMessage, attemptedValue))
        {
            PropertyName = propertyName;
            ErrorMessage = errorMessage;
            AttemptedValue = attemptedValue;
        }

        private static string FormatMessage(string propertyName, string errorMessage, object attemptedValue)
        {
            var format = "Property '{0}' failed to set value '{1}': {2}";
            if (attemptedValue == null)
            {
                format = "Property '{0}' failed to set value to null: {2}";
            }
            return String.Format(format, propertyName, attemptedValue, errorMessage);
        }

        public string PropertyName { get; private set; }

        public string ErrorMessage { get; private set; }

        public object AttemptedValue { get; private set; }
    }
}