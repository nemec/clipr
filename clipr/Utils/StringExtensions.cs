using System.Collections.Generic;
using System.Linq;

namespace clipr.Utils
{
    /// <summary>
    /// 
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// <para>
        /// Strip all newlines from the input string, then return chunks
        /// of text of at most <paramref name="maxChars"/> characters.
        /// </para>
        /// </summary>
        /// <param name="text">String to reflow.</param>
        /// <param name="maxChars">Maximum number of characters per line.</param>
        /// <returns></returns>
        public static IEnumerable<string> Reflow(this string text, int maxChars)
        {
            return ReflowWords(text, maxChars, null);
        }

        /// <summary>
        /// <para>
        /// Strip all newlines from the input string, then return chunks
        /// of text of at most <paramref name="maxChars"/> characters.
        /// </para>
        /// <para>
        /// If a word (defined as consecutive characters that aren't
        /// spaces or tabs) is being split,
        /// the line will return up to the last whole word. If a word's
        /// length is longer than <paramref name="maxChars"/>, it will
        /// be forced to split across multiple lines.
        /// </para>
        /// <para>
        /// If multiple separator characters occur at a line boundary, they
        /// will be removed.
        /// </para>
        /// </summary>
        /// <param name="text">String to reflow.</param>
        /// <param name="maxChars">Maximum number of characters per line.</param>
        /// <returns></returns>
        public static IEnumerable<string> ReflowWords(this string text, int maxChars)
        {
            return ReflowWords(text, maxChars, new[] {' ', '\t'});
        }

        /// <summary>
        /// <para>
        /// Strip all newlines from the input string, then return chunks
        /// of text of at most <paramref name="maxChars"/> characters.
        /// </para>
        /// <para>
        /// If a word (defined as consecutive characters that aren't
        /// given in <paramref name="separators"/>) is being split,
        /// the line will return up to the last whole word. If a word's
        /// length is longer than <paramref name="maxChars"/>, it will
        /// be forced to split across multiple lines.
        /// </para>
        /// <para>
        /// If multiple separator characters occur at a line boundary, they
        /// will be removed.
        /// </para>
        /// </summary>
        /// <param name="text">String to reflow.</param>
        /// <param name="maxChars">Maximum number of characters per line.</param>
        /// <param name="separators">Array of valid separator characters.</param>
        /// <returns></returns>
        public static IEnumerable<string> ReflowWords(this string text, int maxChars,
                                                      char[] separators)
        {
            if (text == null)
            {
                yield break;
            }

            if (separators == null)
            {
                separators = new char[0];
            }

            // Remove newlines
            text = text
                .Replace("\r\n", " ")
                .Replace("\n", " ");

            var start = FindNextNonSeparator(text, 0, separators);
            for (var end = start; end < text.Length; end++)
            {
                if (end - start < maxChars) continue;
                
                // There's a separator at the boundary
                if (separators.Contains(text[end]))
                {
                    var str = text.Substring(start,
                        FindPreviousSeparator(
                            text, start, end, separators) - start);
                    if (str.Length > 0)
                    {
                        yield return str;
                    }
                }
                // We're splitting a word up, step backwards to the beginning
                // of the word.
                else
                {
                    var tmpEnd = FindPreviousNonSeparator(text, start, end, separators);

                    // The word must be longer than the line.
                    // Force a split.
                    if (tmpEnd == start)
                    {
                        var str = text.Substring(start, end - start);
                        if (str.Length > 0)
                        {
                            yield return str;
                        }
                    }
                    else
                    {
                        end = tmpEnd;
                        var str = text.Substring(start,
                            FindPreviousSeparator(
                            text, start, tmpEnd, separators) - start);
                        if (str.Length > 0)
                        {
                            yield return str;
                        }
                    }
                }
                start = FindNextNonSeparator(text, end, separators);
            }
            if (start != text.Length)
            {
                var end = text.Length - 1;
                var tmpEnd = FindPreviousNonSeparator(
                    text, start, end, separators);

                if (tmpEnd == start)
                {
                    var str = text.Substring(start);
                    if (str.Length > 0)
                    {
                        yield return str;
                    }
                }
                else
                {
                    var str = text.Substring(start,
                        FindPreviousSeparator(
                        text, start, tmpEnd, separators) - start);
                    if (str.Length > 0)
                    {
                        yield return str;
                    }
                }
            }
        }

        private static int FindNextNonSeparator(string text, int start, char[] seps)
        {
            while (start < text.Length && seps.Contains(text[start]))
            {
                start++;
            }
            return start;
        }

        private static int FindPreviousSeparator(string text, int start, int end, char[] seps)
        {
            while (end > start && seps.Contains(text[end - 1]))
            {
                end--;
            }
            return end;
        }

        private static int FindPreviousNonSeparator(string text, int start, int end, char[] seps)
        {
            if (end == text.Length - 1 ||
                    seps.Contains(text[end]) &&
                    !seps.Contains(text[end - 1]))
            {
                return end + 1;
            }

            // Step backwards to the beginning of the word
            while (end > start && !seps.Contains(text[end - 1]))
            {
                end--;
            }
            return end;
        }
    }
}
