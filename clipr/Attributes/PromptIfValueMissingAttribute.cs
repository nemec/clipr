
using System;

namespace clipr
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PromptIfValueMissingAttribute : Attribute
    {
        public bool MaskInput { get; set; }
    }
}
