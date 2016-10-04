
using System;

namespace clipr
{
    /// <summary>
    /// When a value is missing for this argument, prompt for it in the console.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class PromptIfValueMissingAttribute : Attribute
    {
        /// <summary>
        /// Prevent the input from showing up in the console while it's being typed.
        /// </summary>
        public bool MaskInput { get; set; }

        /// <summary>
        /// String to signal the parser that the parameter value should be entered at the console.
        /// </summary>
        public string SignalString { get; set; }
    }
}
