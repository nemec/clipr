using System;

namespace clipr
{
    /// <summary>
    /// Localize the description of this item
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false)]
    public class LocalizeAttribute : Attribute
    {
        /// <summary>
        /// The type containing the resources for this item. If this attribute
        /// is applied to a property and the enclosing class has a
        /// <see cref="LocalizeAttribute"/>, the ResourceType will be
        /// inherited unless otherwise specified.
        /// </summary>
        public Type ResourceType { get; set; }

        /// <summary>
        /// The name of the resx resource that holds the translation. If not
        /// provided, the resource name defaults to 'ClassName' if applied to
        /// a class or 'ClassName_PropertyName' if applied to a property.
        /// </summary>
        public string ResourceName { get; set; }

        /// <summary>
        /// Localize the description of this item
        /// </summary>
        /// <param name="resourceName">
        /// 
        /// </param>
        /// <param name="resourceType">
        /// </param>
        public LocalizeAttribute(string resourceName = null, Type resourceType=null)
        {
            ResourceName = resourceName;
            ResourceType = resourceType;
        }
    }
}
