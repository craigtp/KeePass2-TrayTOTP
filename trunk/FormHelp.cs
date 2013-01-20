using System;
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
        private TrayTotpGTExt plugin = null;
        /// <summary>
        /// KeePass Host.
        /// </summary>
        private IPluginHost m_host = null;
        /// <summary>
        /// Getting started flag.
        /// </summary>
        private bool _GettingStarted;

        /// <summary>
        /// Windows Form Constructor.
        /// </summary>
        /// <param name="Plugin">Plugin Host.</param>
        /// <param name="GettingStarted">Getting Started Display Flag.</param>
        internal FormHelp(TrayTotpGTExt Plugin, bool GettingStarted = false)
        {
            plugin = Plugin;
            m_host = plugin.m_host;
            _GettingStarted = GettingStarted;
            InitializeComponent();
        }

        /// <summary>
        /// Windows Form Load.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormHelp_Load(object sender, EventArgs e)
        {
            this.Size = new Size(550, 350);
            this.Text = TrayTotpGTExt.strHelp + " - " + TrayTotpGTExt.strTrayTotpPlugin;
            foreach (Control Ctl in SplitContainerHelp.Panel2.Controls)
            {
                Ctl.Location = new Point(3,3);
                Ctl.Size = new Size(SplitContainerHelp.Panel2.Width - 3, SplitContainerHelp.Panel2.Height - 3);
            }
            TreeViewHelp.ExpandAll();
            TreeViewHelp.SelectedNode = _GettingStarted ? TreeViewHelp.Nodes["GettingStarted"] : TreeViewHelp.Nodes["Welcome"];
        }

        /// <summary>
        /// Windows Form Closing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormHelp_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
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
                    default:
                        break;
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
