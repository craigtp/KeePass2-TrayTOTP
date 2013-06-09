﻿using System;
using System.Reflection;
using System.Windows.Forms;
using KeePass.Plugins;

namespace TrayTotpGT
{
    /// <summary>
    /// Form providing information on the plugin.
    /// </summary>
    internal partial class FormAbout : Form
    {
        /// <summary>
        /// Plugin Host.
        /// </summary>
        private readonly TrayTotpGTExt plugin;
        /// <summary>
        /// KeePass Host.
        /// </summary>
        private readonly IPluginHost m_host;

        /// <summary>
        /// Windows Form Constructor.
        /// </summary>
        /// <param name="Plugin">Plugin Host.</param>
        internal FormAbout(TrayTotpGTExt Plugin)
        {
            plugin = Plugin;
            m_host = plugin.m_host;
            InitializeComponent();
        }

        /// <summary>
        /// Windows Form Load.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormAbout_Load(object sender, EventArgs e)
        {
            this.Text = TrayTotpGTExt.strAbout + TrayTotpGTExt.strSpaceDashSpace + TrayTotpGTExt.strTrayTotpPlugin;
            ListViewAbout.Items[0].SubItems.Add(AssemblyTitle);
            ListViewAbout.Items[1].SubItems.Add(AssemblyCompany);
            ListViewAbout.Items[2].SubItems.Add(AssemblyVersion);
            ListViewAbout.Items[3].SubItems.Add(TrayTotpGTExt.strBuildDate);
            ListViewAbout.Items[4].SubItems.Add(TrayTotpGTExt.strEmail);
            LabelCopyright.Text = AssemblyCopyright;
        }

        /// <summary>
        /// Gets the assembly's title
        /// </summary>
        internal string AssemblyTitle
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != string.Empty)
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        /// <summary>
        /// Gets the assembly's version
        /// </summary>
        internal string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        /// <summary>
        /// Gets the assembly's description
        /// </summary>
        internal string AssemblyDescription
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return string.Empty;
                }
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        /// <summary>
        /// Gets the assembly's product name
        /// </summary>
        internal string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return string.Empty;
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        /// <summary>
        /// Gets the assembly's copyright
        /// </summary>
        internal string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return string.Empty;
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        /// <summary>
        /// Gets the assembly's company name
        /// </summary>
        internal string AssemblyCompany
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return string.Empty;
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }
    }
}