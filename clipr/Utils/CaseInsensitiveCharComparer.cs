using System;
using System.Collections.Generic;

namespace clipr.Utils
{
    internal class CaseInsensitiveCharComparer : IEqualityComparer<char>
    {
        public bool Equals(char x, char y)
        {
            return Char.ToLowerInvariant(x) == Char.ToLowerInvariant(y);
        }

        public int GetHashCode(char obj)
        {
            return Char.ToLowerInvariant(obj).GetHashCode();
        }
    }
}
