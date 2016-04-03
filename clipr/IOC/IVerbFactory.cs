using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace clipr.IOC
{
    public interface IVerbFactory
    {
        object GetVerb(Type objectType);
    }
}
