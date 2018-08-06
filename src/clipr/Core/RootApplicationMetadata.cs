using System;

namespace clipr.Core
{
    internal class RootApplicationMetadata
    {
        public RootApplicationMetadata(
            string applicationName,
            Type resourceType,
            Type rootOptionType)
        {
            ApplicationName = applicationName;
            ResourceType = resourceType;
            RootOptionType = rootOptionType;
        }

        public string ApplicationName { get; private set; }

        public Type ResourceType { get; private set; }

        public Type RootOptionType { get; private set; }
    }
}
