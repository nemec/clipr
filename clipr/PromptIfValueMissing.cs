using clipr.Utils;
using System;
using System.Linq;
using System.Text;

namespace clipr
{
    public class PromptIfValueMissing
    {
        public bool Enabled { get; set; }

        public bool MaskInput { get; set; }

        public string Prompt(string forArgument)
        {
            return MaskInput
                ? InsecureMaskedPrompt(forArgument)
                : PromptNoMask(forArgument);
        }
        
        public string PromptNoMask(string forArgument)
        {
            Console.Write(String.Format(I18N._("PromptIfValueMissing_Prompt") + " ", forArgument));
            return Console.ReadLine();
        }

        public string InsecureMaskedPrompt(string forArgument)
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
