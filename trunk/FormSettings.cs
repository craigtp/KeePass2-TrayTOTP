using System;
using System.ComponentModel;
using System.Windows.Forms;
using KeePass.Plugins;

namespace TrayTotpGT
{
    /// <summary>
    /// Form providing controls to customize the plugin's settings.
    /// </summary>
    internal partial class FormSettings : Form
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
        /// Windows Form Constructor.
        /// </summary>
        /// <param name="Plugin">Plugin Host.</param>
        internal FormSettings(TrayTotpGTExt Plugin)
        {
            plugin = Plugin; //Defines variable from argument.
            m_host = plugin.m_host; //Defines variable from argument.
            InitializeComponent(); //Form Initialization.
        }

        /// <summary>
        /// Contains last network connection status. Is true by default to control user prompt.
        /// </summary>
        private bool NetworkWasConnected = true;

        /// <summary>
        /// Windows Form Load.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormSettings_Load(object sender, EventArgs e)
        {
            this.Text = TrayTotpGTExt.strSettings + " - " + TrayTotpGTExt.strTrayTotpPlugin; //Set form's name using constants.
            Working(true); //Set controls depending on the state of action.
            WorkerLoad.RunWorkerAsync(); //Load Settings in form controls.
        }

        /// <summary>
        /// Windows Form Closing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormSettings_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (WorkerLoad.IsBusy)
            {
                ButtonCancel.Enabled = false;
                WorkerLoad.CancelAsync();
                e.Cancel = true;
            }
            if (WorkerSave.IsBusy)
            {
                ButtonCancel.Enabled = false;
                WorkerSave.CancelAsync();
                e.Cancel = true;
            }
            if (WorkerReset.IsBusy)
            {
                ButtonCancel.Enabled = false;
                WorkerReset.CancelAsync();
                e.Cancel = true;
            }
        }

        private void ButtonReset_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to reset all the settings to their default values?", TrayTotpGTExt.strTrayTotpPlugin, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                Working(true); //Set controls depending on the state of action.
                WorkerReset.RunWorkerAsync();
            }
        }

        private void CheckBoxAutoType_CheckedChanged(object sender, EventArgs e)
        {
            GroupBoxAutoType.Enabled = CheckBoxAutoType.Checked;
        }

        private void CheckBoxAutoTypeFieldName_CheckedChanged(object sender, EventArgs e)
        {
            TextBoxAutoTypeFieldName.Enabled = CheckBoxAutoTypeFieldName.Checked;
            if (!CheckBoxAutoTypeFieldName.Checked)
            {
                CheckBoxAutoTypeFieldRename.Checked = false;
            }
        }

        private void CheckBoxAutoTypeFieldRename_CheckedChanged(object sender, EventArgs e)
        {
            if (CheckBoxAutoTypeFieldRename.Checked)
            {
                if (!m_host.MainWindow.ActiveDatabase.IsOpen)
                {
                    CheckBoxAutoTypeFieldRename.Checked = false;
                    if (m_host.MainWindow.IsFileLocked(null))
                    {
                        MessageBox.Show("The currently opened database is locked! This function only works with an unlocked database.", TrayTotpGTExt.strTrayTotpPlugin, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    MessageBox.Show("You must have an opened database in order to use this function.", TrayTotpGTExt.strTrayTotpPlugin, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (!CheckBoxAutoTypeFieldName.Checked)
                {
                    CheckBoxAutoTypeFieldRename.Checked = false;
                    MessageBox.Show("Please start by enabling \"Field rename\".", TrayTotpGTExt.strTrayTotpPlugin, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
        }

        private void CheckBoxTimeCorrection_CheckedChanged(object sender, EventArgs e)
        {
            GroupBoxTimeCorrection.Enabled = CheckBoxTimeCorrection.Checked;
            GroupBoxTimeCorrectonList.Enabled = CheckBoxTimeCorrection.Checked;
        }

        private void TabControlSettings_Selected(object sender, TabControlEventArgs e)
        {
            if (e.TabPage.Name == "TabPageSync")
            {
                ToolStripButtonRefreshTimeCorrection_Click(null, null);
            }
        }

        private void ListViewTimeCorrectionList_DoubleClick(object sender, EventArgs e)
        {
            ToolStripButtonPropertiesTimeCorrection_Click(sender, e);
        }

        private void ToolStripButtonAddTimeCorrection_Click(object sender, EventArgs e)
        {
            var FormTC = new FormTimeCorrection(plugin);
            if (FormTC.ShowDialog() == DialogResult.OK)
            {
                ListViewTimeCorrectionList.Items.Add(FormTC.ComboBoxUrlTimeCorrection.Text, 0);
            }
        }

        private void ToolStripButtonPropertiesTimeCorrection_Click(object sender, EventArgs e)
        {
            if (ListViewTimeCorrectionList.SelectedItems.Count == 1)
            {
                ListViewItem ThisItem = ListViewTimeCorrectionList.SelectedItems[0];
                var FormTC = new FormTimeCorrection(plugin, ThisItem.Text);
                if (FormTC.ShowDialog() == DialogResult.OK)
                {
                    ThisItem.SubItems[0].Text = FormTC.ComboBoxUrlTimeCorrection.Text;
                    ThisItem.SubItems[1].Text = string.Empty;
                    ThisItem.ImageIndex = 0;
                }
            }
        }

        private void ToolStripButtonRemoveTimeCorrection_Click(object sender, EventArgs e)
        {
            if (ListViewTimeCorrectionList.SelectedItems.Count == 1)
            {
                ListViewTimeCorrectionList.SelectedItems[0].Remove();
            }
        }

        private void ToolStripButtonRefreshTimeCorrection_Click(object sender, EventArgs e)
        {
            ListViewTimeCorrectionList.Items.Clear();
            ListViewTimeCorrectionList.Items.AddRange(plugin.TimeCorrections.ToLVI());
            if (!plugin.NetworkIsConnected)
            {
                if (NetworkWasConnected) MessageBox.Show("Warning, no internet connection detected!\n\nYou will not be able to add any server until you are connected.", TrayTotpGTExt.strTrayTotpPlugin, MessageBoxButtons.OK, MessageBoxIcon.Information);
                ToolStripButtonAddTimeCorrection.Enabled = false;
                ToolStripButtonPropertiesTimeCorrection.Enabled = false;
                NetworkWasConnected = false;
            }
            else
            {
                ToolStripButtonAddTimeCorrection.Enabled = true;
                ToolStripButtonPropertiesTimeCorrection.Enabled = true;
                NetworkWasConnected = true;
            }
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            if (HasErrors()) return;
            Working(true); //Set controls depending on the state of action.
            WorkerSave.RunWorkerAsync("OK");
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            //Dialog Result = Cancel
        }

        private void ButtonApply_Click(object sender, EventArgs e)
        {
            if (HasErrors()) return;
            Working(true); //Set controls depending on the state of action.
            WorkerSave.RunWorkerAsync();
        }

        private bool HasErrors()
        {
            bool temp = false;
            ErrorProviderSettings.SetError(ButtonOK, string.Empty);
            ErrorProviderSettings.SetError(TextBoxAutoTypeFieldName, string.Empty);
            ErrorProviderSettings.SetError(ComboBoxTotpSeedStringName, string.Empty);
            ErrorProviderSettings.SetError(ComboBoxTotpSettingsStringName, string.Empty);
            if (TextBoxAutoTypeFieldName.Text.Contains("{") || TextBoxAutoTypeFieldName.Text.Contains("}")) ErrorProviderSettings.SetError(TextBoxAutoTypeFieldName, "Invalid character!");
            if (ComboBoxTotpSeedStringName.Text == ComboBoxTotpSettingsStringName.Text)
            {
                ErrorProviderSettings.SetError(ComboBoxTotpSeedStringName, "Invalid name! Must be different than the Setting String.");
                ErrorProviderSettings.SetError(ComboBoxTotpSettingsStringName, "Invalid name! Must be different than the Seed String.");
            }
            if (ErrorProviderSettings.GetError(TextBoxAutoTypeFieldName) != string.Empty) temp = true;
            if (ErrorProviderSettings.GetError(ComboBoxTotpSeedStringName) != string.Empty) temp = true;
            if (ErrorProviderSettings.GetError(ComboBoxTotpSettingsStringName) != string.Empty) temp = true;
            if (temp) ErrorProviderSettings.SetError(ButtonOK, "Errors detected!");
            return temp;
        }

        private void Working(bool Enable)
        {
            this.UseWaitCursor = Enable;
            TabControlSettings.Enabled = !Enable;
            ButtonReset.Enabled = false;//!Enable;
            ButtonOK.Enabled = !Enable;
            ButtonApply.Enabled = !Enable;
        }

        private void WorkerLoad_DoWork(object sender, DoWorkEventArgs e)
        {
            //Argument
            e.Result = e.Argument;

            //Menus
            CheckBoxShowCopyTotpEntryMenu.Checked = m_host.CustomConfig.GetBool(TrayTotpGTExt.setname_bool_EntryContextCopy_Visible, true);
            CheckBoxShowSetupTotpEntryMenu.Checked = m_host.CustomConfig.GetBool(TrayTotpGTExt.setname_bool_EntryContextSetup_Visible, true);
            CheckBoxShowTotpEntriesTrayMenu.Checked = m_host.CustomConfig.GetBool(TrayTotpGTExt.setname_bool_NotifyContext_Visible, true);
            if (WorkerLoad.CancellationPending) { e.Cancel = true; return; }

            //TOTP Column
            CheckBoxTotpColumnClipboard.Checked = m_host.CustomConfig.GetBool(TrayTotpGTExt.setname_bool_TotpColumnCopy_Enable, true);
            CheckBoxTotpColumnTimer.Checked = m_host.CustomConfig.GetBool(TrayTotpGTExt.setname_bool_TotpColumnTimer_Visible, true);
            if (WorkerLoad.CancellationPending) { e.Cancel = true; return; }

            //Auto-Type
            CheckBoxAutoType.Checked = m_host.CustomConfig.GetBool(TrayTotpGTExt.setname_bool_AutoType_Enable, true);
            TextBoxAutoTypeFieldName.Text = m_host.CustomConfig.GetString(TrayTotpGTExt.setname_string_AutoType_FieldName, TrayTotpGTExt.setdef_string_AutoType_FieldName);
            if (WorkerLoad.CancellationPending) { e.Cancel = true; return; }

            //Time Correction
            CheckBoxTimeCorrection.Checked = m_host.CustomConfig.GetBool(TrayTotpGTExt.setname_bool_TimeCorrection_Enable, false);
            NumericTimeCorrectionInterval.Value = Convert.ToDecimal(m_host.CustomConfig.GetULong(TrayTotpGTExt.setname_ulong_TimeCorrection_RefreshTime, TrayTotpGTExt.setdef_ulong_TimeCorrection_RefreshTime));
            ListViewTimeCorrectionList.Items.AddRange(plugin.TimeCorrections.ToLVI());
            if (WorkerLoad.CancellationPending) { e.Cancel = true; return; }

            //Storage
            if (m_host.MainWindow.ActiveDatabase.IsOpen)
            {
                foreach (var pe in m_host.MainWindow.ActiveDatabase.RootGroup.GetEntries(true))
                {
                    foreach (var str in pe.Strings)
                    {
                        if (!ComboBoxTotpSeedStringName.Items.Contains(str.Key))
                        {
                            ComboBoxTotpSeedStringName.Items.Add(str.Key);
                            ComboBoxTotpSettingsStringName.Items.Add(str.Key);
                        }
                    }
                }
            }
            ComboBoxTotpSeedStringName.Text = m_host.CustomConfig.GetString(TrayTotpGTExt.setname_string_TotpSeed_StringName, TrayTotpGTExt.setdef_string_TotpSeed_StringName);
            ComboBoxTotpSettingsStringName.Text = m_host.CustomConfig.GetString(TrayTotpGTExt.setname_string_TotpSettings_StringName, TrayTotpGTExt.setdef_string_TotpSettings_StringName);
            if (WorkerLoad.CancellationPending) e.Cancel = true;
        }

        private void WorkerLoad_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
            else
            {
                Working(false); //Set controls depending on the state of action.
                if (e.Result != null)
                {
                    if (e.Result.ToString() == "Reset")
                    {
                        MessageBox.Show("Settings restored to defaults!", TrayTotpGTExt.strTrayTotpPlugin, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private void WorkerSave_DoWork(object sender, DoWorkEventArgs e)
        {
            //Argument
            e.Result = e.Argument;

            //Menus
            m_host.CustomConfig.SetBool(TrayTotpGTExt.setname_bool_EntryContextCopy_Visible, CheckBoxShowCopyTotpEntryMenu.Checked);
            m_host.CustomConfig.SetBool(TrayTotpGTExt.setname_bool_EntryContextSetup_Visible, CheckBoxShowSetupTotpEntryMenu.Checked);
            m_host.CustomConfig.SetBool(TrayTotpGTExt.setname_bool_NotifyContext_Visible, CheckBoxShowTotpEntriesTrayMenu.Checked);
            if (WorkerSave.CancellationPending) { e.Cancel = true; return; }

            //TOTP Column
            m_host.CustomConfig.SetBool(TrayTotpGTExt.setname_bool_TotpColumnCopy_Enable, CheckBoxTotpColumnClipboard.Checked);
            m_host.CustomConfig.SetBool(TrayTotpGTExt.setname_bool_TotpColumnTimer_Visible, CheckBoxTotpColumnTimer.Checked);
            if (WorkerSave.CancellationPending) { e.Cancel = true; return; }

            //Auto-Type
            m_host.CustomConfig.SetBool(TrayTotpGTExt.setname_bool_AutoType_Enable, CheckBoxAutoType.Checked);
            if (CheckBoxAutoTypeFieldName.Checked)
            {
                string OldAutoTypeFieldName = m_host.CustomConfig.GetString(TrayTotpGTExt.setname_string_AutoType_FieldName, TrayTotpGTExt.setdef_string_AutoType_FieldName).ExtWithBrackets();
                string NewAutoTypeFieldName = TextBoxAutoTypeFieldName.Text.ExtWithBrackets();
                KeePass.Util.Spr.SprEngine.FilterPlaceholderHints.Remove(OldAutoTypeFieldName);
                if (CheckBoxAutoTypeFieldRename.Checked) //Replace existing field of custom keystrokes from all entries and all groups
                {
                    m_host.MainWindow.ActiveDatabase.RootGroup.DefaultAutoTypeSequence = m_host.MainWindow.ActiveDatabase.RootGroup.DefaultAutoTypeSequence.Replace(OldAutoTypeFieldName, NewAutoTypeFieldName);
                    foreach (var group in m_host.MainWindow.ActiveDatabase.RootGroup.GetGroups(true))
                    {
                        group.DefaultAutoTypeSequence = group.DefaultAutoTypeSequence.Replace(OldAutoTypeFieldName, NewAutoTypeFieldName);
                        foreach (var pe in group.GetEntries(false))
                        {
                            foreach (var Association in pe.AutoType.Associations)
                            {
                                Association.Sequence = Association.Sequence.Replace(OldAutoTypeFieldName, NewAutoTypeFieldName);
                            }
                        }
                    }
                }
                m_host.CustomConfig.SetString(TrayTotpGTExt.setname_string_AutoType_FieldName, TextBoxAutoTypeFieldName.Text);
                KeePass.Util.Spr.SprEngine.FilterPlaceholderHints.Add(NewAutoTypeFieldName);
            }
            if (WorkerSave.CancellationPending) { e.Cancel = true; return; }

            //Time Correction
            m_host.CustomConfig.SetBool(TrayTotpGTExt.setname_bool_TimeCorrection_Enable, CheckBoxTimeCorrection.Checked);
            plugin.TimeCorrections.Enable = CheckBoxTimeCorrection.Checked;
            m_host.CustomConfig.SetULong(TrayTotpGTExt.setname_ulong_TimeCorrection_RefreshTime, Convert.ToUInt64(NumericTimeCorrectionInterval.Value));
            OtpProviderClient.TimeCorrection_Provider.Interval = Convert.ToInt16(NumericTimeCorrectionInterval.Value);
            plugin.TimeCorrections.ResetThenAddRangeFromLVIs(ListViewTimeCorrectionList.Items);
            m_host.CustomConfig.SetString(TrayTotpGTExt.setname_string_TimeCorrection_List, plugin.TimeCorrections.ToSetting());
            if (WorkerSave.CancellationPending) { e.Cancel = true; return; }

            //Storage
            m_host.CustomConfig.SetString(TrayTotpGTExt.setname_string_TotpSeed_StringName, ComboBoxTotpSeedStringName.Text);
            m_host.CustomConfig.SetString(TrayTotpGTExt.setname_string_TotpSettings_StringName, ComboBoxTotpSettingsStringName.Text);
            if (WorkerSave.CancellationPending) e.Cancel = true;
        }

        private void WorkerSave_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
            else
            {
                Working(false); //Set controls depending on the state of action.
                if (e.Result != null) if (e.Result.ToString() == "OK") this.DialogResult = DialogResult.OK;
            }
        }

        private void WorkerReset_DoWork(object sender, DoWorkEventArgs e)
        {
            //Menus
            m_host.CustomConfig.SetString(TrayTotpGTExt.setname_bool_EntryContextCopy_Visible, null);
            m_host.CustomConfig.SetString(TrayTotpGTExt.setname_bool_EntryContextSetup_Visible, null);
            m_host.CustomConfig.SetString(TrayTotpGTExt.setname_bool_NotifyContext_Visible, null);
            if (WorkerReset.CancellationPending) { e.Cancel = true; return; }

            //TOTP Column
            m_host.CustomConfig.SetString(TrayTotpGTExt.setname_bool_TotpColumnCopy_Enable, null);
            m_host.CustomConfig.SetString(TrayTotpGTExt.setname_bool_TotpColumnTimer_Visible, null);
            if (WorkerReset.CancellationPending) { e.Cancel = true; return; }

            //Auto-Type
            m_host.CustomConfig.SetString(TrayTotpGTExt.setname_bool_AutoType_Enable, null);
            string OldAutoTypeFieldName = m_host.CustomConfig.GetString(TrayTotpGTExt.setname_string_AutoType_FieldName, TrayTotpGTExt.setdef_string_AutoType_FieldName).ExtWithBrackets();
            string NewAutoTypeFieldName = TrayTotpGTExt.strTotp.ExtWithBrackets();
            KeePass.Util.Spr.SprEngine.FilterPlaceholderHints.Remove(OldAutoTypeFieldName);
            m_host.MainWindow.ActiveDatabase.RootGroup.DefaultAutoTypeSequence = m_host.MainWindow.ActiveDatabase.RootGroup.DefaultAutoTypeSequence.Replace(OldAutoTypeFieldName, NewAutoTypeFieldName);
            foreach (var group in m_host.MainWindow.ActiveDatabase.RootGroup.GetGroups(true))
            {
                group.DefaultAutoTypeSequence = group.DefaultAutoTypeSequence.Replace(OldAutoTypeFieldName, NewAutoTypeFieldName);
                foreach (var pe in group.GetEntries(false))
                {
                    foreach (var Association in pe.AutoType.Associations)
                    {
                        Association.Sequence = Association.Sequence.Replace(OldAutoTypeFieldName, NewAutoTypeFieldName);
                    }
                }
            }
            m_host.CustomConfig.SetString(TrayTotpGTExt.setname_string_AutoType_FieldName, null);
            KeePass.Util.Spr.SprEngine.FilterPlaceholderHints.Add(NewAutoTypeFieldName);
            if (WorkerReset.CancellationPending) { e.Cancel = true; return; }

            //Time Correction
            m_host.CustomConfig.SetString(TrayTotpGTExt.setname_bool_TimeCorrection_Enable, null);
            m_host.CustomConfig.SetString(TrayTotpGTExt.setname_ulong_TimeCorrection_RefreshTime, null);
            OtpProviderClient.TimeCorrection_Provider.Interval = Convert.ToInt16(TrayTotpGTExt.setdef_ulong_TimeCorrection_RefreshTime);
            plugin.TimeCorrections.ResetThenAddRangeFromString(string.Empty);
            if (WorkerReset.CancellationPending) { e.Cancel = true; return; }

            //Storage
            m_host.CustomConfig.SetString(TrayTotpGTExt.setname_string_TotpSeed_StringName, null);
            m_host.CustomConfig.SetString(TrayTotpGTExt.setname_string_TotpSettings_StringName, null);
            if (WorkerReset.CancellationPending) e.Cancel = true;
        }

        private void WorkerReset_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
            else
            {
                WorkerLoad.RunWorkerAsync("Reset");
            }
        }
    }
}