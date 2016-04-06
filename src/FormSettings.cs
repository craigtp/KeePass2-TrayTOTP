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
        private readonly TrayTotpGTExt _plugin;

        /// <summary>
        /// Windows Form Constructor.
        /// </summary>
        /// <param name="pPLUGIN">Plugin Host.</param>
        internal FormSettings(TrayTotpGTExt pPLUGIN)
        {
            _plugin = pPLUGIN; //Defines variable from argument.
            InitializeComponent(); //Form Initialization.
        }

        /// <summary>
        /// Contains last network connection status. Is true by default to control user prompt.
        /// </summary>
        private bool _networkWasConnected = true;

        /// <summary>
        /// Windows Form Load.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormSettings_Load(object sender, EventArgs e)
        {
            Text = TrayTotp_Plugin_Localization.strSettings + TrayTotp_Plugin_Localization.strSpaceDashSpace + TrayTotp_Plugin_Localization.strTrayTotpPlugin; //Set form's name using constants.
            Working(true, true); //Set controls depending on the state of action.
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
            if (MessageBox.Show(FormSettings_Localization.SettingsAskResetDefaultValues, TrayTotp_Plugin_Localization.strTrayTotpPlugin, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                Working(true, false); //Set controls depending on the state of action.
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
                if (!_plugin.m_host.MainWindow.ActiveDatabase.IsOpen)
                {
                    CheckBoxAutoTypeFieldRename.Checked = false;
                    if (_plugin.m_host.MainWindow.IsFileLocked(null))
                    {
                        MessageBox.Show(FormSettings_Localization.SettingsCurrentDatabaseLocked, TrayTotp_Plugin_Localization.strTrayTotpPlugin, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    MessageBox.Show(FormSettings_Localization.SettingsOpenDatabaseRequired, TrayTotp_Plugin_Localization.strTrayTotpPlugin, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (!CheckBoxAutoTypeFieldName.Checked)
                {
                    CheckBoxAutoTypeFieldRename.Checked = false;
                    MessageBox.Show(FormSettings_Localization.SettingsEnableFieldRename, TrayTotp_Plugin_Localization.strTrayTotpPlugin, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            var FormTC = new FormTimeCorrection(_plugin);
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
                var FormTC = new FormTimeCorrection(_plugin, ThisItem.Text);
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
            ListViewTimeCorrectionList.Items.AddRange(_plugin.TimeCorrections.ToLVI());
            if (!_plugin.NetworkIsConnected)
            {
                if (_networkWasConnected) MessageBox.Show(string.Format(FormSettings_Localization.SettingsNoInternetDetected, Environment.NewLine), TrayTotp_Plugin_Localization.strTrayTotpPlugin, MessageBoxButtons.OK, MessageBoxIcon.Information);
                ToolStripButtonAddTimeCorrection.Enabled = false;
                ToolStripButtonPropertiesTimeCorrection.Enabled = false;
                _networkWasConnected = false;
            }
            else
            {
                ToolStripButtonAddTimeCorrection.Enabled = true;
                ToolStripButtonPropertiesTimeCorrection.Enabled = true;
                _networkWasConnected = true;
            }
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            if (HasErrors()) return;
            Working(true, true); //Set controls depending on the state of action.
            WorkerSave.RunWorkerAsync("OK");
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            //Dialog Result = Cancel
        }

        private void ButtonApply_Click(object sender, EventArgs e)
        {
            if (HasErrors()) return;
            Working(true, true); //Set controls depending on the state of action.
            WorkerSave.RunWorkerAsync();
        }

        private bool HasErrors()
        {
            bool temp = false;
            ErrorProviderSettings.SetError(ButtonOK, string.Empty);
            ErrorProviderSettings.SetError(TextBoxAutoTypeFieldName, string.Empty);
            ErrorProviderSettings.SetError(ComboBoxTotpSeedStringName, string.Empty);
            ErrorProviderSettings.SetError(ComboBoxTotpSettingsStringName, string.Empty);
            if (TextBoxAutoTypeFieldName.Text.Contains("{") || TextBoxAutoTypeFieldName.Text.Contains("}")) ErrorProviderSettings.SetError(TextBoxAutoTypeFieldName, FormSettings_Localization.SettingsInvalidCharacter);
            if (ComboBoxTotpSeedStringName.Text == ComboBoxTotpSettingsStringName.Text)
            {
                ErrorProviderSettings.SetError(ComboBoxTotpSeedStringName, FormSettings_Localization.SettingsInvalidNameSetting);
                ErrorProviderSettings.SetError(ComboBoxTotpSettingsStringName, FormSettings_Localization.SettingsInvalidNameSeed);
            }
            if (ErrorProviderSettings.GetError(TextBoxAutoTypeFieldName) != string.Empty) temp = true;
            if (ErrorProviderSettings.GetError(ComboBoxTotpSeedStringName) != string.Empty) temp = true;
            if (ErrorProviderSettings.GetError(ComboBoxTotpSettingsStringName) != string.Empty) temp = true;
            if (temp) ErrorProviderSettings.SetError(ButtonOK, FormSettings_Localization.SettingsErrors);
            return temp;
        }

        private void Working(bool Enable, bool Cancellable)
        {
            UseWaitCursor = Enable;
            TabControlSettings.Enabled = !Enable;
            ButtonReset.Enabled = !Enable;
            ButtonOK.Enabled = !Enable;
            ButtonCancel.Enabled = Cancellable;
            ButtonApply.Enabled = !Enable;
        }

        private void WorkerLoad_DoWork(object sender, DoWorkEventArgs e)
        {
            //Argument
            e.Result = e.Argument;

            //Menus
            CheckBoxShowCopyTotpEntryMenu.Checked = _plugin.m_host.CustomConfig.GetBool(TrayTotpGTExt.setname_bool_EntryContextCopy_Visible, true);
            CheckBoxShowSetupTotpEntryMenu.Checked = _plugin.m_host.CustomConfig.GetBool(TrayTotpGTExt.setname_bool_EntryContextSetup_Visible, true);
            CheckBoxShowTotpEntriesTrayMenu.Checked = _plugin.m_host.CustomConfig.GetBool(TrayTotpGTExt.setname_bool_NotifyContext_Visible, true);
            if (WorkerLoad.CancellationPending) { e.Cancel = true; return; }

            //TOTP Column
            CheckBoxTotpColumnClipboard.Checked = _plugin.m_host.CustomConfig.GetBool(TrayTotpGTExt.setname_bool_TotpColumnCopy_Enable, true);
            CheckBoxTotpColumnTimer.Checked = _plugin.m_host.CustomConfig.GetBool(TrayTotpGTExt.setname_bool_TotpColumnTimer_Visible, true);
            if (WorkerLoad.CancellationPending) { e.Cancel = true; return; }

            //Auto-Type
            CheckBoxAutoType.Checked = _plugin.m_host.CustomConfig.GetBool(TrayTotpGTExt.setname_bool_AutoType_Enable, true);
            TextBoxAutoTypeFieldName.Text = _plugin.m_host.CustomConfig.GetString(TrayTotpGTExt.setname_string_AutoType_FieldName, TrayTotpGTExt.setdef_string_AutoType_FieldName);
            if (WorkerLoad.CancellationPending) { e.Cancel = true; return; }

            //Time Correction
            CheckBoxTimeCorrection.Checked = _plugin.m_host.CustomConfig.GetBool(TrayTotpGTExt.setname_bool_TimeCorrection_Enable, false);
            NumericTimeCorrectionInterval.Value = Convert.ToDecimal(_plugin.m_host.CustomConfig.GetULong(TrayTotpGTExt.setname_ulong_TimeCorrection_RefreshTime, TrayTotpGTExt.setdef_ulong_TimeCorrection_RefreshTime));
            ListViewTimeCorrectionList.Items.AddRange(_plugin.TimeCorrections.ToLVI());
            if (WorkerLoad.CancellationPending) { e.Cancel = true; return; }

            //Storage
            if (_plugin.m_host.MainWindow.ActiveDatabase.IsOpen)
            {
                foreach (var pe in _plugin.m_host.MainWindow.ActiveDatabase.RootGroup.GetEntries(true))
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
            ComboBoxTotpSeedStringName.Text = _plugin.m_host.CustomConfig.GetString(TrayTotpGTExt.setname_string_TotpSeed_StringName, TrayTotp_Plugin_Localization.setdef_string_TotpSeed_StringName);
            ComboBoxTotpSettingsStringName.Text = _plugin.m_host.CustomConfig.GetString(TrayTotpGTExt.setname_string_TotpSettings_StringName, TrayTotp_Plugin_Localization.setdef_string_TotpSettings_StringName);
            if (WorkerLoad.CancellationPending) e.Cancel = true;
        }

        private void WorkerLoad_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
            else
            {
                Working(false, true); //Set controls depending on the state of action.
                if (e.Result != null)
                {
                    if (e.Result.ToString() == "Reset")
                    {
                        MessageBox.Show(FormSettings_Localization.SettingsDefaultValuesRestored, TrayTotp_Plugin_Localization.strTrayTotpPlugin, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private void WorkerSave_DoWork(object sender, DoWorkEventArgs e)
        {
            //Argument
            e.Result = e.Argument;

            //Menus
            _plugin.m_host.CustomConfig.SetBool(TrayTotpGTExt.setname_bool_EntryContextCopy_Visible, CheckBoxShowCopyTotpEntryMenu.Checked);
            _plugin.m_host.CustomConfig.SetBool(TrayTotpGTExt.setname_bool_EntryContextSetup_Visible, CheckBoxShowSetupTotpEntryMenu.Checked);
            _plugin.m_host.CustomConfig.SetBool(TrayTotpGTExt.setname_bool_NotifyContext_Visible, CheckBoxShowTotpEntriesTrayMenu.Checked);
            if (WorkerSave.CancellationPending) { e.Cancel = true; return; }

            //TOTP Column
            _plugin.m_host.CustomConfig.SetBool(TrayTotpGTExt.setname_bool_TotpColumnCopy_Enable, CheckBoxTotpColumnClipboard.Checked);
            _plugin.m_host.CustomConfig.SetBool(TrayTotpGTExt.setname_bool_TotpColumnTimer_Visible, CheckBoxTotpColumnTimer.Checked);
            if (WorkerSave.CancellationPending) { e.Cancel = true; return; }

            //Auto-Type
            _plugin.m_host.CustomConfig.SetBool(TrayTotpGTExt.setname_bool_AutoType_Enable, CheckBoxAutoType.Checked);
            if (CheckBoxAutoTypeFieldName.Checked)
            {
                string OldAutoTypeFieldName = _plugin.m_host.CustomConfig.GetString(TrayTotpGTExt.setname_string_AutoType_FieldName, TrayTotpGTExt.setdef_string_AutoType_FieldName).ExtWithBrackets();
                string NewAutoTypeFieldName = TextBoxAutoTypeFieldName.Text.ExtWithBrackets();
                KeePass.Util.Spr.SprEngine.FilterPlaceholderHints.Remove(OldAutoTypeFieldName);
                if (CheckBoxAutoTypeFieldRename.Checked) //Replace existing field of custom keystrokes from all entries and all groups
                {
                    _plugin.m_host.MainWindow.ActiveDatabase.RootGroup.DefaultAutoTypeSequence = _plugin.m_host.MainWindow.ActiveDatabase.RootGroup.DefaultAutoTypeSequence.Replace(OldAutoTypeFieldName, NewAutoTypeFieldName);
                    foreach (var group in _plugin.m_host.MainWindow.ActiveDatabase.RootGroup.GetGroups(true))
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
                _plugin.m_host.CustomConfig.SetString(TrayTotpGTExt.setname_string_AutoType_FieldName, TextBoxAutoTypeFieldName.Text);
                KeePass.Util.Spr.SprEngine.FilterPlaceholderHints.Add(NewAutoTypeFieldName);
            }
            if (WorkerSave.CancellationPending) { e.Cancel = true; return; }

            //Time Correction
            _plugin.m_host.CustomConfig.SetBool(TrayTotpGTExt.setname_bool_TimeCorrection_Enable, CheckBoxTimeCorrection.Checked);
            _plugin.TimeCorrections.Enable = CheckBoxTimeCorrection.Checked;
            _plugin.m_host.CustomConfig.SetULong(TrayTotpGTExt.setname_ulong_TimeCorrection_RefreshTime, Convert.ToUInt64(NumericTimeCorrectionInterval.Value));
            OtpProviderClient.TimeCorrection_Provider.Interval = Convert.ToInt16(NumericTimeCorrectionInterval.Value);
            _plugin.TimeCorrections.ResetThenAddRangeFromLVIs(ListViewTimeCorrectionList.Items);
            _plugin.m_host.CustomConfig.SetString(TrayTotpGTExt.setname_string_TimeCorrection_List, _plugin.TimeCorrections.ToSetting());
            if (WorkerSave.CancellationPending) { e.Cancel = true; return; }

            //Storage
            _plugin.m_host.CustomConfig.SetString(TrayTotpGTExt.setname_string_TotpSeed_StringName, ComboBoxTotpSeedStringName.Text);
            _plugin.m_host.CustomConfig.SetString(TrayTotpGTExt.setname_string_TotpSettings_StringName, ComboBoxTotpSettingsStringName.Text);
            if (WorkerSave.CancellationPending) e.Cancel = true;
        }

        private void WorkerSave_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
            else
            {
                Working(false, true); //Set controls depending on the state of action.
                if (e.Result != null) if (e.Result.ToString() == "OK") DialogResult = DialogResult.OK;
            }
        }

        private void WorkerReset_DoWork(object sender, DoWorkEventArgs e)
        {
            //Menus
            _plugin.m_host.CustomConfig.SetString(TrayTotpGTExt.setname_bool_EntryContextCopy_Visible, null);
            _plugin.m_host.CustomConfig.SetString(TrayTotpGTExt.setname_bool_EntryContextSetup_Visible, null);
            _plugin.m_host.CustomConfig.SetString(TrayTotpGTExt.setname_bool_NotifyContext_Visible, null);

            //TOTP Column
            _plugin.m_host.CustomConfig.SetString(TrayTotpGTExt.setname_bool_TotpColumnCopy_Enable, null);
            _plugin.m_host.CustomConfig.SetString(TrayTotpGTExt.setname_bool_TotpColumnTimer_Visible, null);

            //Auto-Type
            _plugin.m_host.CustomConfig.SetString(TrayTotpGTExt.setname_bool_AutoType_Enable, null);
            string OldAutoTypeFieldName = _plugin.m_host.CustomConfig.GetString(TrayTotpGTExt.setname_string_AutoType_FieldName, TrayTotpGTExt.setdef_string_AutoType_FieldName).ExtWithBrackets();
            string NewAutoTypeFieldName = TrayTotp_Plugin_Localization.strTotp.ExtWithBrackets();
            KeePass.Util.Spr.SprEngine.FilterPlaceholderHints.Remove(OldAutoTypeFieldName);
            _plugin.m_host.MainWindow.ActiveDatabase.RootGroup.DefaultAutoTypeSequence = _plugin.m_host.MainWindow.ActiveDatabase.RootGroup.DefaultAutoTypeSequence.Replace(OldAutoTypeFieldName, NewAutoTypeFieldName);
            foreach (var group in _plugin.m_host.MainWindow.ActiveDatabase.RootGroup.GetGroups(true))
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
            _plugin.m_host.CustomConfig.SetString(TrayTotpGTExt.setname_string_AutoType_FieldName, null);
            KeePass.Util.Spr.SprEngine.FilterPlaceholderHints.Add(NewAutoTypeFieldName);

            //Time Correction
            _plugin.m_host.CustomConfig.SetString(TrayTotpGTExt.setname_bool_TimeCorrection_Enable, null);
            _plugin.m_host.CustomConfig.SetString(TrayTotpGTExt.setname_ulong_TimeCorrection_RefreshTime, null);
            OtpProviderClient.TimeCorrection_Provider.Interval = Convert.ToInt16(TrayTotpGTExt.setdef_ulong_TimeCorrection_RefreshTime);
            _plugin.TimeCorrections.ResetThenAddRangeFromString(string.Empty);

            //Storage
            _plugin.m_host.CustomConfig.SetString(TrayTotpGTExt.setname_string_TotpSeed_StringName, null);
            _plugin.m_host.CustomConfig.SetString(TrayTotpGTExt.setname_string_TotpSettings_StringName, null);
        }

        private void WorkerReset_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            WorkerLoad.RunWorkerAsync("Reset");
        }
    }
}