using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace clipr.Validation
{
    public class AllowedRangeAttribute : ValidationAttribute
    {
        private const string TypeHandleErrorMessage = 
            "The AllowedRangeAttribute can only validate primitive integer or floating point types";

        public static readonly Type[] AllowedTypes = new[]
        {
            typeof(byte), typeof(sbyte),
            typeof(short), typeof(ushort),
            typeof(int), typeof(uint),
            typeof(long), typeof(ulong),
            typeof(float), typeof(double),
        };

        /// <summary>
        /// The minimum allowed value in the range, inclusive.
        /// </summary>
        public double MinValue {
            get { return _minValue.GetValueOrDefault(); }
            set { _minValue = value; }
        }
        private double? _minValue { get; set; }

        /// <summary>
        /// The maximum allowed value in the range, exclusive.
        /// </summary>
        public double MaxValue {
            get { return _maxValue.GetValueOrDefault(); }
            set { _maxValue = value; }
        }
        private double? _maxValue;

        public override bool CanHandleType(Type type, out string errorMessage)
        {
            if (!AllowedTypes.Contains(type))
            {
                errorMessage = TypeHandleErrorMessage;
                return false;
            }
            errorMessage = null;
            return true;
        }

        public override bool ValidateMember(object member, out Exception error)
        {
            var validationFailure = Failure.None;
            double doubleValue = 0;

            switch (member)
            {
                case byte b:
                    doubleValue = b;
                    break;
                case sbyte b:
                    doubleValue = b;
                    break;
                case int i:
                    doubleValue = i;
                    break;
                case uint i:
                    doubleValue = i;
                    break;
                case short s:
                    doubleValue = s;
                    break;
                case ushort s:
                    doubleValue = s;
                    break;
                case long l:
                    doubleValue = l;
                    break;
                case ulong l:
                    doubleValue = l;
                    break;
                case float f:
                    doubleValue = f;
                    break;
                case double d:
                    doubleValue = d;
                    break;
                default:
                    throw new ArgumentException(String.Format("Invalid type {0} provided", member.GetType()));
            }

            if (_minValue.HasValue && doubleValue < _minValue)
            {
                validationFailure = Failure.Below;
            }
            // exclusive compare, must check AboutEquals against double
            else if (_maxValue.HasValue &&
                (doubleValue > _maxValue || AboutEqual(doubleValue, _maxValue.Value)))
            {
                validationFailure = Failure.Above;
            }

            switch (validationFailure)
            {
                case Failure.None:
                    error = null;
                    return true;
                case Failure.Above:
                    error = new ArgumentException(
                        String.Format("The value {0} is above the allowed range.", member));
                    return false;
                case Failure.Below:
                    error = new ArgumentException(
                        String.Format("The value {0} is below the allowed range.", member));
                    return false;
            }
            error = new Exception("Unknown AllowedRange failure");
            return false;
        }

        private static bool AboutEqual(double x, double y)
        {
            double epsilon = Math.Max(Math.Abs(x), Math.Abs(y)) * 1E-15;
            return Math.Abs(x - y) <= epsilon;
        }

        private const double Epsilon = 0.000000001;

        private enum Failure
        {
            None,
            Above,
            Below
        }
    }
}
