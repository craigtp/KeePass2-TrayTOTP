﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.32559
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TrayTotpGT {
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
    internal class TrayTotp_CustomColumn_Localization {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal TrayTotp_CustomColumn_Localization() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("TrayTotpGT.TrayTotp_CustomColumn_Localization", typeof(TrayTotp_CustomColumn_Localization).Assembly);
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
        ///   Looks up a localized string similar to TOTP.
        /// </summary>
        internal static string strTotp {
            get {
                return ResourceManager.GetString("strTotp", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error, bad seed!.
        /// </summary>
        internal static string strWarningBadSeed {
            get {
                return ResourceManager.GetString("strWarningBadSeed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error, bad settings!.
        /// </summary>
        internal static string strWarningBadSet {
            get {
                return ResourceManager.GetString("strWarningBadSet", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Warning, bad URL?.
        /// </summary>
        internal static string strWarningBadUrl {
            get {
                return ResourceManager.GetString("strWarningBadUrl", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error, storage!.
        /// </summary>
        internal static string strWarningStorage {
            get {
                return ResourceManager.GetString("strWarningStorage", resourceCulture);
            }
        }
    }
}
