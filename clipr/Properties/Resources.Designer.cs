﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace clipr.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("clipr.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Display this help document..
        /// </summary>
        internal static string AutomaticHelpGenerator_Description {
            get {
                return ResourceManager.GetString("AutomaticHelpGenerator_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Optional Arguments.
        /// </summary>
        internal static string AutomaticHelpGenerator_NamedArgumentsTitle {
            get {
                return ResourceManager.GetString("AutomaticHelpGenerator_NamedArgumentsTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Positional Arguments.
        /// </summary>
        internal static string AutomaticHelpGenerator_PositionalArgumentsTitle {
            get {
                return ResourceManager.GetString("AutomaticHelpGenerator_PositionalArgumentsTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Usage.
        /// </summary>
        internal static string AutomaticHelpGenerator_Usage {
            get {
                return ResourceManager.GetString("AutomaticHelpGenerator_Usage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Displays the version of the current executable..
        /// </summary>
        internal static string ExecutingAssemblyVersion_Description {
            get {
                return ResourceManager.GetString("ExecutingAssemblyVersion_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Input a value for {0}:.
        /// </summary>
        internal static string PromptIfValueMissing_Prompt {
            get {
                return ResourceManager.GetString("PromptIfValueMissing_Prompt", resourceCulture);
            }
        }
    }
}
