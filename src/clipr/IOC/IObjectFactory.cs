using System;

namespace clipr.IOC
{
    public interface IObjectFactory
    {
        bool CanCreateObject(Type objectType);

        object CreateObject(Type objectType);
    }
}
