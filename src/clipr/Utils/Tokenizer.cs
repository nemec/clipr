using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace clipr.Utils
{
    public static class Tokenizer
    {
        // https://msdn.microsoft.com/en-us/library/system.environment.getcommandlineargs(v=vs.110).aspx

        private static readonly char[] Whitespace = new[] { ' ', '\t' };

        public static string[] Tokenize(string commandLine)
        {
            var args = new List<string>();

            var state = State.Normal;
            var sb = new StringBuilder();
            var escapedN = 0;
            for(var i = 0; i < commandLine.Length; i++)
            {
                var c = commandLine[i];
                if (c == '\\')
                {
                    escapedN++;
                    state |= State.Escaped;
                }
                else if (c == '"')
                {
                    if ((state & State.Escaped) != 0)
                    {
                        // Trim quotes in half
                        for (var j = 0; j < escapedN / 2; j++)
                        {
                            sb.Append('\\');
                        }
                        if (escapedN % 2 != 0)
                        {
                            sb.Append(c);
                        }
                        escapedN = 0;
                        state &= ~State.Escaped;
                        continue;
                    }
                    if ((state & State.Quoted) != 0)
                    {
                        state &= ~State.Quoted;
                        continue;
                    }
                    state |= State.Quoted;
                }
                else if (Whitespace.Contains(c))
                {
                    if ((state & State.Quoted) != 0)
                    {
                        sb.Append(c);
                        continue;
                    }
                    if (sb.Length > 0)
                    {
                        args.Add(sb.ToString());
                        sb.Length = 0;
                    }
                }
                else
                {
                    if((state & State.Escaped) != 0)
                    {
                        // Don't trim quotes in half
                        for (var j = 0; j < escapedN; j++)
                        {
                            sb.Append('\\');
                        }
                        escapedN = 0;
                        state &= ~State.Escaped;
                    }
                    sb.Append(c);
                }
            }

            if (sb.Length > 0) args.Add(sb.ToString());

            return args.ToArray();
        }

        [Flags]
        private enum State
        {
            Normal = 0,
            Quoted = 1,
            Escaped = 2
        }
    }
}
