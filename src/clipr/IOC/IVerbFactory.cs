using System;

namespace clipr.IOC
{
    public interface IVerbFactory
    {
        bool CanCreateVerb(Type objectType);

        object GetVerb(Type objectType);
    }
}
