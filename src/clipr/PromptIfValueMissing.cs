using clipr.Utils;
using System;
using System.Linq;
using System.Text;

namespace clipr
{
    /// <summary>
    /// When a value is missing for this argument, prompt for it in the console.
    /// </summary>
    public class PromptIfValueMissing
    {
        /// <summary>
        /// Indicate whether the console should prompt for input if the argument value is missing.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Prevent the input from showing up in the console while it's being typed.
        /// </summary>
        public bool MaskInput { get; set; }

        /// <summary>
        /// String to signal the parser that the parameter value should be entered at the console.
        /// </summary>
        public string SignalString { get; set; }

        /// <summary>
        /// When a value is missing for this argument, prompt for it in the console.
        /// </summary>
        public PromptIfValueMissing()
        {
            SignalString = "-";
        }

        internal string Prompt(string forArgument)
        {
            return MaskInput
                ? InsecureMaskedPrompt(forArgument)
                : PromptNoMask(forArgument);
        }
        
        internal string PromptNoMask(string forArgument)
        {
            Console.Write(String.Format(I18N._("PromptIfValueMissing_Prompt") + " ", forArgument));
            return Console.ReadLine();
        }

        internal string InsecureMaskedPrompt(string forArgument)
        {
            Console.Write(String.Format(I18N._("PromptIfValueMissing_Prompt") + " ", forArgument));
            const int ENTER = 13, BACKSP = 8, CTRLBACKSP = 127;
            int[] FILTERED = { 0, 27, 9, 10 }; // const

            var builder = new StringBuilder();

            char chr = (char)0;

            while ((chr = Console.ReadKey(true).KeyChar) != ENTER)
            {
                if (((chr == BACKSP) || (chr == CTRLBACKSP))
                    && (builder.Length > 0))
                {
                    Console.Write("\b \b");
                    builder.Remove(builder.Length - 1, 1);

                }
                // Don't append * when length is 0 and backspace is selected
                else if (((chr == BACKSP) || (chr == CTRLBACKSP)) && (builder.Length == 0))
                {
                }

                // Don't append when a filtered char is detected
                else if (FILTERED.Count(x => chr == x) > 0)
                {
                }

                // Append and write * mask
                else
                {
                    builder.Append(chr);
                }
            }

            Console.WriteLine();
            return builder.ToString();
            /*IntPtr ptr = new IntPtr();
            ptr = Marshal.SecureStringToBSTR(securePass);
            string plainPass = Marshal.PtrToStringBSTR(ptr);
            Marshal.ZeroFreeBSTR(ptr);
            return plainPass;*/
        }
    }
}
