﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Tql.Plugins.Outlook {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Labels {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Labels() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Tql.Plugins.Outlook.Labels", typeof(Labels).Assembly);
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
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Email.
        /// </summary>
        public static string EmailsMatch_Label {
            get {
                return ResourceManager.GetString("EmailsMatch_Label", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Find people.
        /// </summary>
        public static string EmailsMatch_SearchHint {
            get {
                return ResourceManager.GetString("EmailsMatch_SearchHint", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Outlook isn&apos;t running.
        /// </summary>
        public static string OutlookClient_OutlookIsNotRunning {
            get {
                return ResourceManager.GetString("OutlookClient_OutlookIsNotRunning", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Outlook.
        /// </summary>
        public static string OutlookPeopleDirectory_Label {
            get {
                return ResourceManager.GetString("OutlookPeopleDirectory_Label", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Outlook.
        /// </summary>
        public static string OutlookPlugin_Label {
            get {
                return ResourceManager.GetString("OutlookPlugin_Label", resourceCulture);
            }
        }
    }
}
