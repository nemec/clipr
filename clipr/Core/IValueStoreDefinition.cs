
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace clipr.Core
{
    /// <summary>
    /// Defines the interface for storing a parsed value into the
    /// configuration object.
    /// </summary>
    public interface IValueStoreDefinition
    {
        /// <summary>
        /// Name of the storage area (e.g. property name).
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Additional converters for the object.
        /// </summary>
        /// <value>The converters.</value>
        TypeConverter[] Converters { get; }

        /// <summary>
        /// Set this value of object <paramref name="parent"/>
        /// to <paramref name="value"/>.
        /// </summary>
        /// <param name="parent">Configuration instance</param>
        /// <param name="value">Value to store</param>
        void SetValue(object parent, object value);

        /// <summary>
        /// Gets this value of object <paramref name="parent"/>.
        /// </summary>
        /// <param name="parent">Configuration instance</param>
        /// <returns></returns>
        object GetValue(object parent);

        /// <summary>
        /// Get an attribute of the given type that is bound to the value.
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <returns></returns>
        TAttribute GetCustomAttribute<TAttribute>() where TAttribute : Attribute;

        /// <summary>
        /// Get all attributes of the given type that are bound to the value.
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <returns></returns>
        IEnumerable<TAttribute> GetCustomAttributes<TAttribute>() where TAttribute : Attribute;

        /// <summary>
        /// The value's type.
        /// </summary>
        Type Type { get; }
    }
}
