using System;
using System.Collections.Generic;

namespace clipr.Validation
{
    public class BasicParseValidator<T> : IParseValidator<T>
    {
        private readonly List<Func<T, ValidationFailure>> _rules = new List<Func<T, ValidationFailure>>();

        public BasicParseValidator<T> AddRule(Func<T, ValidationFailure> rule)
        {
            _rules.Add(rule);
            return this;
        }

        public ValidationResult Validate(T component)
        {
            var errs = new List<ValidationFailure>();
            foreach(var rule in _rules)
            {
                errs.Add(rule(component));
            }
            return new ValidationResult(errs);
        }
    }
}