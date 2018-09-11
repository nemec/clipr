using System;
using System.Collections.Generic;
using System.Linq;

namespace clipr.Validation
{
    public class AttributeValidator<TConf> : IParseValidator<TConf>
    {
        private readonly HashSet<Type> _validationAttributes = new HashSet<Type>();

        public static readonly AttributeValidator<TConf> Default;

        static AttributeValidator()
        {
            Default = new AttributeValidator<TConf>();
            Default.AddAttributeValidationType<AllowedRangeAttribute>();
            Default.AddAttributeValidationType<FileExistsAttribute>();
            Default.AddAttributeValidationType<DirectoryExistsAttribute>();
        }

        public void AddAttributeValidationType<T>()
            where T : ValidationAttribute
        {
            _validationAttributes.Add(typeof(T));
        }

        public void AddAttributeValidationType(Type t)
        {
            _validationAttributes.Add(t);
        }

        public ValidationResult Validate(TConf component)
        {
            if(!_validationAttributes.Any())
            {
                return new ValidationResult();
            }

            var failures = new List<ValidationFailure>();

            var props = typeof(TConf).GetProperties();
            foreach(var prop in props)
            {
                var value = prop.GetValue(component, null);
                if (value == null)
                {
                    // No need to validate, it was not filled in (set Required=true to ensure it's filled in).
                    continue;
                }
                var attrs = prop.GetCustomAttributes(true).OfType<ValidationAttribute>();
                foreach(var attr in attrs)
                {
                    if(_validationAttributes.Contains(attr.GetType()))
                    {
                        if (!attr.CanHandleType(prop.PropertyType, out string typeError))
                        {
                            failures.Add(new ValidationFailure(prop.Name, typeError));
                        }
                        else if(!attr.ValidateMember(value, out Exception error))
                        {
                            failures.Add(new ValidationFailure(prop.Name, error.Message, value));
                        }
                    }
                }
            }
            return new ValidationResult(failures);
        }
    }
}
