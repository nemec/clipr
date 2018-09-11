using System;

namespace clipr.Validation
{
    /// <summary>
    /// Define a validator for a given type to be used in the
    /// <see cref="AttributeValidator{TConf}"/>.
    /// </summary>
    public abstract class ValidationAttribute : Attribute
    {
        /// <summary>
        /// Decide whether this validator can handle the type it was applied to.
        /// If this method returns FALSE it SHOULD return an additional error message
        /// describing the error. If it returns TRUE the error message is ignored.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> that this attribute was applied on.</param>
        /// <param name="errorMessage">
        /// Error message that describes why this class cannot handle the type.
        /// </param>
        /// <returns></returns>
        public abstract bool CanHandleType(Type type, out string errorMessage);

        /// <summary>
        /// Perform validation of the property or field. If the member fails validation,
        /// you may return an <see cref="Exception"/> describing the error in detail.
        /// </summary>
        /// <param name="member">
        /// The instance of the property or field that the <see cref="ValidationAttribute"/>
        /// was applied to.
        /// </param>
        /// <param name="error">The error describing the failed validation.</param>
        /// <returns>True if the member passes validation, false if it fails.</returns>
        public abstract bool ValidateMember(object member, out Exception error);
    }
}
