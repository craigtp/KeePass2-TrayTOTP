﻿using System;
using System.Drawing;
using System.Windows.Forms;

using KeePass.Plugins;

namespace TrayTotpGT
{
    /// <summary>
    /// Form providing help information on the Tray Totp Plugin.
    /// </summary>
    internal partial class FormHelp : Form
    {
        /// <summary>
        /// Plugin Host.
        /// </summary>
        private readonly TrayTotpGTExt _plugin;
        /// <summary>
        /// Getting started flag.
        /// </summary>
        private readonly bool _gettingStarted;

        /// <summary>
        /// Windows Form Constructor.
        /// </summary>
        /// <param name="plugin">Plugin Host.</param>
        /// <param name="gettingStarted">Getting Started Display Flag.</param>
        internal FormHelp(TrayTotpGTExt plugin, bool gettingStarted = false)
        {
            _plugin = plugin;
            _gettingStarted = gettingStarted;
            InitializeComponent();
        }

        /// <summary>
        /// Windows Form Load.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormHelp_Load(object sender, EventArgs e)
        {
            Size = new Size(550, 350);
            Text = TrayTotp_Plugin_Localization.strHelp + TrayTotp_Plugin_Localization.strSpaceDashSpace + TrayTotp_Plugin_Localization.strTrayTotpPlugin;
            foreach (Control ctl in SplitContainerHelp.Panel2.Controls)
            {
                ctl.Location = new Point(3,3);
                ctl.Size = new Size(SplitContainerHelp.Panel2.Width - 3, SplitContainerHelp.Panel2.Height - 3);
            }
            TreeViewHelp.ExpandAll();
            TreeViewHelp.SelectedNode = _gettingStarted ? TreeViewHelp.Nodes["GettingStarted"] : TreeViewHelp.Nodes["Welcome"];
        }

        /// <summary>
        /// Windows Form Closing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormHelp_FormClosing(object sender, FormClosingEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }

        /// <summary>
        /// TreeView Node Selection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeViewHelp_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                switch (e.Node.Name)
                {
                    case "Welcome":
                        PanelWelcome.BringToFront();
                        break;
                    case "GettingStarted":
                        PanelGettingStarted.BringToFront();
                        break;
                    case "Components":
                        PanelComponents.BringToFront();
                        break;
                    case "EntryList":
                        PanelEntryList.BringToFront();
                        break;
                    case "ContextMenu":
                        PanelContextMenu.BringToFront();
                        break;
                    case "CopyTotp":
                        PanelCopyTotp.BringToFront();
                        break;
                    case "SetupTotp":
                        PanelSetupTotp.BringToFront();
                        break;
                    case "CustomColumn":
                        PanelCustomColumn.BringToFront();
                        break;
                    case "TrayIcon":
                        PanelTrayIcon.BringToFront();
                        break;
                    case "AutoType":
                        PanelAutoType.BringToFront();
                        break;
                    case "TimeCorrection":
                        PanelTimeCorrection.BringToFront();
                        break;
                    case "Storage":
                        PanelStorage.BringToFront();
                        break;
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
