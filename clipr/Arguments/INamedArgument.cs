using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace clipr.Arguments
{
    public interface INamedArgument : ILongNameArgument, IShortNameArgument
    {
    }
}
