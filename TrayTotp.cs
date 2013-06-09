using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;

using OtpProviderClient;

using KeePass.App;
using KeePass.App.Configuration;
using KeePass.DataExchange;
using KeePass.Forms;
using KeePass.Ecas;
using KeePass.Plugins;
using KeePass.Resources;
using KeePass.UI;
using KeePass.Util;
using KeePass.Util.Spr;

using KeePassLib;
using KeePassLib.Utility;
using KeePassLib.Security;
using KeePassLib.Delegates;
using KeePassLib.Resources;
using KeePassLib.Interfaces;
using KeePassLib.Collections;
using KeePassLib.Serialization;
using KeePassLib.Cryptography.Cipher;
using KeePassLib.Cryptography.PasswordGenerator;

namespace TrayTotpGT
{
    /// <summary>
    /// Main Plugin Class
    /// </summary>
    internal sealed partial class TrayTotpGTExt : Plugin
    {
        /// <summary>
        /// Plugin host Global Reference for access to KeePass functions.
        /// </summary>
        internal IPluginHost m_host = null;

        /// <summary>
        /// Constants (plugin display texts).
        /// </summary>
        internal const string strTrayTotpPlugin = "Tray Totp Plugin";
        internal const string strSettings = "Settings";
        internal const string strTimeCorrection = "Time Correction";
        internal const string strSetup = "Setup";
        internal const string strHelp = "Help";
        internal const string strAbout = "About";
        internal const string strTotp = "TOTP";
        internal const string strCopyTotp = "Copy TOTP";
        internal const string strSetupTotp = "Setup TOTP";
        internal const string strNoTotpFound = "No TOTP Seed found!";
        internal const string strDatabaseNotOpen = "Database is not open!";
        internal const string strDatabaseIsLocked = "Database is locked!";
        internal const string strWarningBadUrl = "Warning, bad URL?";
        internal const string strWarningBadSet = "Error, bad settings!";
        internal const string strWarningBadSeed = "Error, bad seed!";
        internal const string strWarningNotSet = "Error, no settings!";
        internal const string strWarningStorage = "Error, storage!";
        internal const string strBuildDate = "2013/06/08";
        internal const string strEmail = "traytotp@gartech.ca";
        /// <summary>
        /// Constants (plugin display texts).
        /// </summary>
        internal const string strSpaceDashSpace = " - ";
        /// <summary>
        /// Constants (keepass form object names).
        /// </summary>
        internal const string keeobj_string_EntryContextMenuCopyPassword_Name = "m_ctxEntryCopyPassword";
        internal const string keeobj_string_EntryContextMenuEntriesSubMenu_Name = "m_ctxEntryMassModify";
        internal const string keeobj_string_EntryContextMenuEntriesSubMenuSeperator1_Name = "m_ctxEntrySelectedSep1";
        /// <summary>
        /// Constants (custom string key).
        /// </summary>
        internal const string setname_string_TimeCorrection_List = "traytotp_timecorrection_list";
        /// <summary>
        /// Constants (setting names).
        /// </summary>
        internal const string setname_bool_FirstInstall_Shown = "firstinstall_shown";
        internal const string setname_bool_EntryContextCopy_Visible = "entrycontextcopy_visible";
        internal const string setname_bool_EntryContextSetup_Visible = "entrycontextsetup_visible";
        internal const string setname_bool_NotifyContext_Visible = "notifycontext_visible";
        internal const string setname_bool_TotpColumnCopy_Enable = "totpcolumncopy_enable";
        internal const string setname_bool_TotpColumnTimer_Visible = "totpcolumntimer_visible";
        internal const string setname_bool_AutoType_Enable = "autotype_enable";
        internal const string setname_string_AutoType_FieldName = "autotype_fieldname";
        internal const string setname_bool_TimeCorrection_Enable = "timecorrection_enable";
        internal const string setname_ulong_TimeCorrection_RefreshTime = "timecorrection_refreshtime";
        internal const string setname_string_TotpSeed_StringName = "totpseed_stringname";
        internal const string setname_string_TotpSettings_StringName = "totpsettings_stringname";
        /// <summary>
        /// Constants (default settings values).
        /// </summary>
        internal const string setdef_string_AutoType_FieldName = "TOTP";
        internal const long setdef_ulong_TimeCorrection_RefreshTime = 60;
        internal const string setdef_string_TotpSeed_StringName = "TOTP Seed";
        internal const string setdef_string_TotpSettings_StringName = "TOTP Settings";
        /// <summary>
        /// Constants (static settings value).
        /// </summary>
        internal const int setstat_int_EntryList_RefreshRate = 300;

        /// <summary>
        /// Form Help Global Reference.
        /// </summary>
        private FormHelp HelpForm;

        /// <summary>
        /// Tools Menu Tray Totp Plugin.
        /// </summary>
        private ToolStripMenuItem toMenuTrayTotp;
        /// <summary>
        /// Tools Menu Tray Totp Settings.
        /// </summary>
        private ToolStripMenuItem toSubMenuSettings;
        /// <summary>
        /// Tools Menu Tray Totp Settings Seperator.
        /// </summary>
        private ToolStripSeparator toSubMenuSeperator1;
        /// <summary>
        /// Tools Menu Tray Totp Help.
        /// </summary>
        private ToolStripMenuItem toSubMenuHelp;
        /// <summary>
        /// Tools Menu Tray Totp About.
        /// </summary>
        private ToolStripMenuItem toSubMenuAbout;

        /// <summary>
        /// Entry Context Menu Copy.
        /// </summary>
        private ToolStripMenuItem enMenuCopyTotp;
        /// <summary>
        /// Entry Context Menu Setup.
        /// </summary>
        private ToolStripMenuItem enMenuSetupTotp;
        /// <summary>
        /// Entry Context Menu Setup Seperator.
        /// </summary>
        private ToolStripSeparator enMenuSeperator;

        /// <summary>
        /// Notify Icon Context Menu Title.
        /// </summary>
        private ToolStripMenuItem niMenuTitle;
        /// <summary>
        /// Notify Icon Context Menu List.
        /// </summary>
        private List<ToolStripMenuItem> niMenuList = new List<ToolStripMenuItem>();
        /// <summary>
        /// Notify Icon Context Menu Seperator.
        /// </summary>
        private ToolStripSeparator niMenuSeperator = null;

        /// <summary>
        /// Entries Column TOTP.
        /// </summary>
        private TrayTotp_CustomColumn liColumnTotp = null;

        /// <summary>
        /// Entry List Column Count.
        /// </summary>
        private int liColumnsCount = 0;
        /// <summary>
        /// Entry List Column TOTP visibility.
        /// </summary>
        private bool liColumnTotpVisible = false;
        /// <summary>
        /// Entry Groups last selected group.
        /// </summary>
        private PwGroup liGroupsPreviousSelected = null;
        /// <summary>
        /// Entry Column TOTP has TOTPs.
        /// </summary>
        private bool liColumnTotpContains = false;

        /// <summary>
        /// Entries Refresh Timer.
        /// </summary>
        private Timer liRefreshTimer = new Timer();
        /// <summary>
        /// Entries Refresh Timer.
        /// </summary>
        internal int liRefreshTimerInterval
        {
            set
            {
                liRefreshTimer.Interval = value;
                liRefreshTimer.Enabled = true;
            }
        }
        /// <summary>
        /// Entries Refresh Timer Previous Counter to Prevent Useless Refresh.
        /// </summary>
        private int liRefreshTimerPreviousCounter;

        /// <summary>
        /// Time Correction Collection.
        /// </summary>
        internal TimeCorrection_Collection TimeCorrections;

        /// <summary>
        /// Network status of the computer.
        /// </summary>
        internal bool NetworkIsConnected
        {
            get
            {
                return System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
            }
        }

        /// <summary>
        /// Initialization of the plugin, adding menus, handlers and forms.
        /// </summary>
        /// <param name="host">Plugin host for access to KeePass functions.</param>
        /// <returns>Successful loading of the plugin, if not the plugin is removed.</returns>
        public override bool Initialize(IPluginHost host)
        {
            //Internalise Host Handle.
            if (host == null) return false;
            m_host = host;

            //Instanciate Help Form.
            HelpForm = new FormHelp(this);

            //Register form events.
            m_host.MainWindow.Shown += MainWindow_Shown;

            //Tools Menus.
            toMenuTrayTotp = new ToolStripMenuItem(strTrayTotpPlugin);
            toMenuTrayTotp.Image = Properties.Resources.TOTP;
            m_host.MainWindow.ToolsMenu.DropDownItems.Add(toMenuTrayTotp);
            toSubMenuSettings = new ToolStripMenuItem(strSettings);
            toSubMenuSettings.Image = Properties.Resources.TOTP_Settings;
            toSubMenuSettings.Click += OnMenuSettingsClick;
            toMenuTrayTotp.DropDownItems.Add(toSubMenuSettings);
            toSubMenuSeperator1 = new ToolStripSeparator();
            toMenuTrayTotp.DropDownItems.Add(toSubMenuSeperator1);
            toSubMenuHelp = new ToolStripMenuItem(strHelp);
            toSubMenuHelp.Image = Properties.Resources.TOTP_Help;
            toSubMenuHelp.Click += OnMenuHelpClick;
            toMenuTrayTotp.DropDownItems.Add(toSubMenuHelp);
            toSubMenuAbout = new ToolStripMenuItem(strAbout + "...");
            toSubMenuAbout.Image = Properties.Resources.TOTP_Info;
            toSubMenuAbout.Click += OnMenuAboutClick;
            toMenuTrayTotp.DropDownItems.Add(toSubMenuAbout);

            //Entry Context Menus.
            m_host.MainWindow.EntryContextMenu.Opening += OnEntryMenuOpening;
            enMenuCopyTotp = new ToolStripMenuItem(strCopyTotp);
            enMenuCopyTotp.Image = Properties.Resources.TOTP;
            enMenuCopyTotp.ShortcutKeys = (Keys)Shortcut.CtrlT;
            enMenuCopyTotp.Click += OnEntryMenuTotpClick;
            m_host.MainWindow.EntryContextMenu.Items.Insert(m_host.MainWindow.EntryContextMenu.Items.IndexOfKey(keeobj_string_EntryContextMenuCopyPassword_Name) + 1, enMenuCopyTotp);
            enMenuSetupTotp = new ToolStripMenuItem(strSetupTotp);
            enMenuSetupTotp.Image = Properties.Resources.TOTP_Setup;
            enMenuSetupTotp.ShortcutKeys = (Keys)Shortcut.CtrlShiftI;
            enMenuSetupTotp.Click += OnEntryMenuSetupClick;
            var ContextMenu = (ToolStripMenuItem)m_host.MainWindow.EntryContextMenu.Items.Find(keeobj_string_EntryContextMenuEntriesSubMenu_Name, true)[0];
            ContextMenu.DropDownItems.Insert(ContextMenu.DropDownItems.IndexOfKey(keeobj_string_EntryContextMenuEntriesSubMenuSeperator1_Name) + 1, enMenuSetupTotp);
            enMenuSeperator = new ToolStripSeparator();
            ContextMenu.DropDownItems.Insert(ContextMenu.DropDownItems.IndexOf(enMenuSetupTotp) + 1, enMenuSeperator);

            //Notify Icon Context Menus.
            m_host.MainWindow.MainNotifyIcon.ContextMenuStrip.Opening += OnNotifyMenuOpening;
            niMenuTitle = new ToolStripMenuItem(strTrayTotpPlugin);
            niMenuTitle.Image = Properties.Resources.TOTP;
            m_host.MainWindow.MainNotifyIcon.ContextMenuStrip.Items.Insert(0, niMenuTitle);
            niMenuSeperator = new ToolStripSeparator();
            m_host.MainWindow.MainNotifyIcon.ContextMenuStrip.Items.Insert(1, niMenuSeperator);

            //Register auto-type function.
            if (m_host.CustomConfig.GetBool(setname_bool_AutoType_Enable, true))
            {
                SprEngine.FilterCompile += SprEngine_FilterCompile;
                SprEngine.FilterPlaceholderHints.Add(m_host.CustomConfig.GetString(setname_string_AutoType_FieldName, setdef_string_AutoType_FieldName).ExtWithBrackets());
            }

            //List Column TOTP.
            liColumnTotp = new TrayTotp_CustomColumn(this);
            m_host.ColumnProviderPool.Add(liColumnTotp);

            //Refresh Timer.
            liRefreshTimer.Interval = setstat_int_EntryList_RefreshRate;
            liRefreshTimer.Enabled = true;
            liRefreshTimer.Tick += OnTimerTick;

            //Time Correction.
            TimeCorrections = new TimeCorrection_Collection(this, m_host.CustomConfig.GetBool(setname_bool_TimeCorrection_Enable, false));
            TimeCorrection_Provider.Interval = Convert.ToInt16(m_host.CustomConfig.GetULong(TrayTotpGTExt.setname_ulong_TimeCorrection_RefreshTime, TrayTotpGTExt.setdef_ulong_TimeCorrection_RefreshTime));
            TimeCorrections.AddRangeFromList(m_host.CustomConfig.GetString(setname_string_TimeCorrection_List, string.Empty).Split(';').ToList());

            return true;
        }

        /// <summary>
        /// Occurs when the main window is shown.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Shown(object sender, EventArgs e)
        {
            if (!m_host.CustomConfig.GetBool(setname_bool_FirstInstall_Shown, false))
            {
                m_host.CustomConfig.SetBool(setname_bool_FirstInstall_Shown, true);
                if (!HelpForm.Visible)
                {
                    HelpForm = new FormHelp(this, true);
                    HelpForm.Show();
                }
                else
                {
                    HelpForm.Focus();
                }
            }
        }

        /// <summary>
        /// Tools Menu Tray Totp Settings Click.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMenuSettingsClick(object sender, EventArgs e)
        {
            var FormSettings = new FormSettings(this);
            UIUtil.ShowDialogAndDestroy(FormSettings);
        }

        /// <summary>
        /// Tools Menu Tray Totp Help Click.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMenuHelpClick(object sender, EventArgs e)
        {
            if (!HelpForm.Visible)
            {
                HelpForm.Show();
            }
            else
            {
                HelpForm.Focus();
            }
        }

        /// <summary>
        /// Tools Menu Tray Totp About Click.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMenuAboutClick(object sender, EventArgs e)
        {
            var FormAbout = new FormAbout(this);
            UIUtil.ShowDialogAndDestroy(FormAbout);
        }

        /// <summary>
        /// Entry Context Menu Opening.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEntryMenuOpening(object sender, CancelEventArgs e)
        {
            enMenuCopyTotp.Enabled = false;
            enMenuSetupTotp.Enabled = false;

            bool boolCopy = m_host.CustomConfig.GetBool(setname_bool_EntryContextCopy_Visible, true);
            enMenuCopyTotp.Visible = boolCopy;

            if (m_host.MainWindow.GetSelectedEntriesCount() == 1)
            {
                var CurrentEntry = m_host.MainWindow.GetSelectedEntry(true);
                if (SettingsCheck(CurrentEntry) && SeedCheck(CurrentEntry))
                {
                    if (SettingsValidate(CurrentEntry))
                    {
                        enMenuCopyTotp.Enabled = true;
                        enMenuCopyTotp.Tag = CurrentEntry;
                    }
                }
                enMenuSetupTotp.Enabled = true;
            }

            bool boolSetup = m_host.CustomConfig.GetBool(setname_bool_EntryContextSetup_Visible, true);
            enMenuSetupTotp.Visible = boolSetup;
            enMenuSeperator.Visible = boolSetup;
        }

        /// <summary>
        /// Entry Context Menu Copy Click.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEntryMenuTotpClick(object sender, EventArgs e)
        {
            TotpCopyToClipboard(((PwEntry)((ToolStripMenuItem)sender).Tag));
        }

        /// <summary>
        /// Entry Context Menu Setup Click.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEntryMenuSetupClick(object sender, EventArgs e)
        {
            if (m_host.MainWindow.GetSelectedEntriesCount() == 1)
            {
                var FormWizard = new FormSetup(this, m_host.MainWindow.GetSelectedEntry(true));
                FormWizard.ShowDialog();
                m_host.MainWindow.RefreshEntriesList();
            }
        }

        /// <summary>
        /// Notify Icon Context Menu Opening.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnNotifyMenuOpening(object sender, CancelEventArgs e)
        {
            foreach (var Menu in niMenuList)
            {
                m_host.MainWindow.MainNotifyIcon.ContextMenuStrip.Items.Remove(Menu);
            }
            niMenuList.Clear();
            if (m_host.CustomConfig.GetBool(setname_bool_NotifyContext_Visible, true))
            {
                niMenuTitle.Visible = true;
                niMenuSeperator.Visible = true;
                if (m_host.MainWindow.ActiveDatabase.IsOpen)
                {
                    foreach (PwEntry Entry in m_host.MainWindow.ActiveDatabase.RootGroup.GetEntries(true))
                    {
                        if (SettingsCheck(Entry) && SeedCheck(Entry))
                        {
                            var NewMenu = new ToolStripMenuItem(Entry.Strings.ReadSafe(PwDefs.TitleField).ExtWithSpaceAfter() + Entry.Strings.ReadSafe(PwDefs.UserNameField).ExtWithParenthesis(), Properties.Resources.TOTP_Key, OnEntryMenuTotpClick);
                            NewMenu.Tag = Entry;
                            if (!SettingsValidate(Entry))
                            {
                                NewMenu.Enabled = false;
                                NewMenu.Image = Properties.Resources.TOTP_Error;
                            }
                            niMenuList.Add(NewMenu);
                        }
                    }
                    if (niMenuList.Count > 0)
                    {
                        niMenuList.Sort((p1, p2) => string.Compare(p1.Text, p2.Text, true));
                        for (int i = 0; i <= niMenuList.Count - 1; i++)
                        {
                            m_host.MainWindow.MainNotifyIcon.ContextMenuStrip.Items.Insert(i + 1, niMenuList[i]);
                        }
                    }
                    else
                    {
                        var NewMenu = new ToolStripMenuItem(strNoTotpFound);
                        NewMenu.Image = Properties.Resources.TOTP_None;
                        niMenuList.Add(NewMenu);
                        m_host.MainWindow.MainNotifyIcon.ContextMenuStrip.Items.Insert(1, niMenuList[0]);
                    }
                }
                else
                {
                    if (m_host.MainWindow.IsFileLocked(null))
                    {
                        var NewMenu = new ToolStripMenuItem(strDatabaseIsLocked);
                        NewMenu.Image = Properties.Resources.TOTP_Lock;
                        niMenuList.Add(NewMenu);
                        m_host.MainWindow.MainNotifyIcon.ContextMenuStrip.Items.Insert(1, niMenuList[0]);
                    }
                    else
                    {
                        var NewMenu = new ToolStripMenuItem(strDatabaseNotOpen);
                        NewMenu.Image = Properties.Resources.TOTP_Error;
                        niMenuList.Add(NewMenu);
                        m_host.MainWindow.MainNotifyIcon.ContextMenuStrip.Items.Insert(1, niMenuList[0]);
                    }
                }
            }
            else
            {
                niMenuTitle.Visible = false;
                niMenuSeperator.Visible = false;
            }
        }

        /// <summary>
        /// Timer Event that occurs to refresh the entry list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTimerTick(object sender, EventArgs e)
        {
            if ((m_host.MainWindow.ActiveDatabase.IsOpen) && (m_host.MainWindow.Visible))
            {
                if (KeePass.Program.Config.MainWindow.EntryListColumns.Count != liColumnsCount)
                {
                    liColumnTotpVisible = false;
                    liColumnsCount = KeePass.Program.Config.MainWindow.EntryListColumns.Count;
                    foreach (var Column in KeePass.Program.Config.MainWindow.EntryListColumns)
                    {
                        if (Column.Type == AceColumnType.PluginExt)
                        {
                            if (Column.CustomName == strTotp)
                            {
                                liColumnTotpVisible = true;
                            }
                        }
                    }
                }

                if (liColumnTotpVisible)
                {
                    PwGroup SelectedGroup = m_host.MainWindow.GetSelectedGroup();
                    if (SelectedGroup != liGroupsPreviousSelected)
                    {
                        liColumnTotpContains = false;
                        liGroupsPreviousSelected = SelectedGroup;
                        foreach (var Entry in SelectedGroup.GetEntries(true))
                        {
                            if (SettingsCheck(Entry) && SeedCheck(Entry))
                            {
                                liColumnTotpContains = true;
                            }
                        }
                    }
                }

                if ((liColumnTotpVisible) && (liColumnTotpContains)) //Tests if displayed entries have totps that require refreshing.
                {
                    var CurrentSeconds = DateTime.Now.Second;
                    if (liRefreshTimerPreviousCounter != CurrentSeconds)
                    {
                        m_host.MainWindow.RefreshEntriesList();
                        liRefreshTimerPreviousCounter = CurrentSeconds;
                    }
                }
            }
        }

        /// <summary>
        /// Auto-Type Function.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SprEngine_FilterCompile(object sender, SprEventArgs e)
        {
            if ((e.Context.Flags & SprCompileFlags.ExtActive) == SprCompileFlags.ExtActive)
            {
                if (e.Text.IndexOf(m_host.CustomConfig.GetString(setname_string_AutoType_FieldName, setdef_string_AutoType_FieldName).ExtWithBrackets(), StringComparison.InvariantCultureIgnoreCase) >= 0)
                {
                    if (SettingsCheck(e.Context.Entry) && SeedCheck(e.Context.Entry))
                    {
                        bool ValidInterval = false; bool ValidLength = false; bool ValidUrl = false;
                        if (SettingsValidate(e.Context.Entry, out ValidInterval, out ValidLength, out ValidUrl))
                        {
                            bool NoTimeCorrection = false;
                            string[] Settings = SettingsGet(e.Context.Entry);
                            var TotpGenerator = new Totp_Provider(Convert.ToInt16(Settings[0]), Convert.ToInt16(Settings[1]));
                            if (ValidUrl)
                            {
                                var CurrentTimeCorrection = TimeCorrections[Settings[2]];
                                if (CurrentTimeCorrection != null)
                                {
                                    TotpGenerator.TimeCorrection = CurrentTimeCorrection.TimeCorrection;
                                }
                                else
                                {
                                    TotpGenerator.TimeCorrection = TimeSpan.Zero;
                                    NoTimeCorrection = true;
                                }
                            }
                            string InvalidCharacters;
                            if (SeedValidate(e.Context.Entry, out InvalidCharacters))
                            {
                                e.Context.Entry.Touch(false);
                                e.Text = StrUtil.ReplaceCaseInsensitive(e.Text, m_host.CustomConfig.GetString(setname_string_AutoType_FieldName,setdef_string_AutoType_FieldName).ExtWithBrackets(),TotpGenerator.Generate(Base32.Decode(SeedGet(e.Context.Entry).ReadString().ExtWithoutSpaces())));
                            }
                            else
                            {
                                e.Text = string.Empty;
                                MessageService.ShowWarning(strWarningBadSeed + InvalidCharacters.ExtWithParenthesis().ExtWithSpaceBefore());
                            }
                            if (NoTimeCorrection) MessageService.ShowWarning(strWarningBadUrl);
                        }
                        else
                        {
                            e.Text = string.Empty;
                            MessageService.ShowWarning(strWarningBadSet);
                        }
                    }
                    else
                    {
                        e.Text = string.Empty;
                        MessageService.ShowWarning(strWarningNotSet);
                    }
                }
            }
        }

        /// <summary>
        /// Check if specified Entry contains Settings that are not null.
        /// </summary>
        /// <param name="pe">Pasword Entry.</param>
        /// <returns>Presence of Settings.</returns>
        internal bool SettingsCheck(PwEntry pe)
        {
            return pe.Strings.Exists(m_host.CustomConfig.GetString(setname_string_TotpSettings_StringName, setdef_string_TotpSettings_StringName));
        }

        /// <summary>
        /// Check if specified Entry's Interval and Length are valid.
        /// </summary>
        /// <param name="pe">Password Entry.</param>
        /// <returns>Error(s) while validating Interval or Length.</returns>
        internal bool SettingsValidate(PwEntry pe)
        {
            bool ValidInterval; bool ValidLength ; bool ValidUrl; //Dummies
            return SettingsValidate(pe, out ValidInterval, out ValidLength, out ValidUrl);
        }

        /// <summary>
        /// Check if specified Entry's Interval and Length are valid. The URL status is available as an out boolean.
        /// </summary>
        /// <param name="pe">Password Entry.</param>
        /// <param name="IsUrlValid">Url Validity.</param>
        /// <returns>Error(s) while validating Interval or Length.</returns>
        internal bool SettingsValidate(PwEntry pe, out bool IsUrlValid)
        {
            bool ValidInterval; bool ValidLength; //Dummies
            return SettingsValidate(pe, out ValidInterval, out ValidLength, out IsUrlValid);
        }

        /// <summary>
        /// Check if specified Entry's Interval and Length are valid. All settings statuses are available as out booleans.
        /// </summary>
        /// <param name="pe">Password Entry.</param>
        /// <param name="IsIntervalValid">Interval Validity.</param>
        /// <param name="IsLengthValid">Length Validity.</param>
        /// <param name="IsUrlValid">Url Validity.</param>
        /// <returns>Error(s) while validating Interval or Length.</returns>
        internal bool SettingsValidate(PwEntry pe, out bool IsIntervalValid, out bool IsLengthValid, out bool IsUrlValid)
        {
            bool SettingsValid = true;
            try
            {
                string[] Settings = SettingsGet(pe);
                try
                {
                    IsIntervalValid = (Convert.ToInt16(Settings[0]) > 0) && (Convert.ToInt16(Settings[0]) < 61); //Interval
                }
                catch (Exception)
                {
                    IsIntervalValid = false;
                    SettingsValid = false;
                }
                try
                {
                    IsLengthValid = (Settings[1] == "6") || (Settings[1] == "8"); //Length
                }
                catch (Exception)
                {
                    IsLengthValid = false;
                    SettingsValid = false;
                }
                try
                {
                    IsUrlValid = (Settings[2].StartsWith("http://")) || (Settings[2].StartsWith("https://")); //Url
                }
                catch (Exception)
                {
                    IsUrlValid = false;
                }
            }
            catch (Exception)
            {
                IsIntervalValid = false;
                IsLengthValid = false;
                IsUrlValid = false;
                SettingsValid = false;
            }
            return SettingsValid;
        }

        /// <summary>
        /// Get the entry's Settings using the string name specified in the settings (or default).
        /// </summary>
        /// <param name="pe">Password Entry.</param>
        /// <returns>String Array (Interval, Length, Url).</returns>
        internal string[] SettingsGet(PwEntry pe)
        {
            return pe.Strings.Get(m_host.CustomConfig.GetString(setname_string_TotpSettings_StringName, setdef_string_TotpSettings_StringName)).ReadString().Split(';');
        }

        /// <summary>
        /// Check if the specified Entry contains a Seed.
        /// </summary>
        /// <param name="pe">Password Entry.</param>
        /// <returns>Presence of the Seed.</returns>
        internal bool SeedCheck(PwEntry pe)
        {
            return pe.Strings.Exists(m_host.CustomConfig.GetString(setname_string_TotpSeed_StringName, setdef_string_TotpSeed_StringName));
        }

        /// <summary>
        /// Validates the entry's Seed making sure it's a valid Base32 string.
        /// </summary>
        /// <param name="PasswordEntry">Password Entry.</param>
        /// <returns>Validity of the Seed's characters for Base32 format.</returns>
        internal bool SeedValidate(PwEntry PasswordEntry)
        {
            string InvalidCharacters;
            return SeedGet(PasswordEntry).ReadString().ExtWithoutSpaces().ExtIsBase32(out InvalidCharacters);
        }

        /// <summary>
        /// Validates the entry's Seed making sure it's a valid Base32 string. Invalid characters are available as out string.
        /// </summary>
        /// <param name="PasswordEntry">Password Entry.</param>
        /// <param name="InvalidChars">Password Entry.</param>
        /// <returns>Validity of the Seed's characters.</returns>
        internal bool SeedValidate(PwEntry PasswordEntry, out string InvalidChars)
        {
            return SeedGet(PasswordEntry).ReadString().ExtWithoutSpaces().ExtIsBase32(out InvalidChars);
        }

        /// <summary>
        /// Get the entry's Seed using the string name specified in the settings (or default).
        /// </summary>
        /// <param name="pe">Password Entry.</param>
        /// <returns>Protected Seed.</returns>
        internal ProtectedString SeedGet(PwEntry pe)
        {
            return pe.Strings.Get(m_host.CustomConfig.GetString(setname_string_TotpSeed_StringName, setdef_string_TotpSeed_StringName));
        }

        /// <summary>
        /// Copies the specified entry's generated TOTP to the clipboard using the KeePass's clipboard function.
        /// </summary>
        /// <param name="pe">Password Entry.</param>
        private void TotpCopyToClipboard(PwEntry pe)
        {
            if (SettingsCheck(pe) && SeedCheck(pe))
            {
                bool ValidInterval; bool ValidLength; bool ValidUrl;
                if (SettingsValidate(pe, out ValidInterval, out ValidLength, out ValidUrl))
                {
                    bool NoTimeCorrection = false;
                    string[] Settings = SettingsGet(pe);
                    var TotpGenerator = new Totp_Provider(Convert.ToInt16(Settings[0]), Convert.ToInt16(Settings[1]));
                    if (ValidUrl)
                    {
                        var CurrentTimeCorrection = TimeCorrections[Settings[2]];
                        if (CurrentTimeCorrection != null)
                        {
                            TotpGenerator.TimeCorrection = CurrentTimeCorrection.TimeCorrection;
                        }
                        else
                        {
                            TotpGenerator.TimeCorrection = TimeSpan.Zero;
                            NoTimeCorrection = true;
                        }
                    }
                    string InvalidCharacters;
                    if (SeedValidate(pe, out InvalidCharacters))
                    {
                        pe.Touch(false);
                        ClipboardUtil.CopyAndMinimize(TotpGenerator.Generate(Base32.Decode(SeedGet(pe).ReadString().ExtWithoutSpaces())), true, m_host.MainWindow, pe, m_host.MainWindow.ActiveDatabase);
                        m_host.MainWindow.StartClipboardCountdown();
                    }
                    else
                    {
                        MessageService.ShowWarning(strWarningBadSeed + InvalidCharacters.ExtWithParenthesis().ExtWithSpaceBefore());
                    }
                    if (NoTimeCorrection) MessageService.ShowWarning(strWarningBadUrl);
                }
                else
                {
                    MessageService.ShowWarning(strWarningBadSet);
                }
            }
            else
            {
                MessageService.ShowWarning(strWarningNotSet);
            }
        }

        /// <summary>
        /// Removes all ressources such as menus, handles and forms from KeePass.
        /// </summary>
        public override void Terminate()
        {
            //Destroy Help Form.
            if (HelpForm.Visible)
            {
                HelpForm.Close();
            }
            else
            {
                HelpForm.Dispose();
            }

            //Unregister internal events.
            m_host.MainWindow.Shown -= MainWindow_Shown;

            //Remove Tools Menus.
            m_host.MainWindow.ToolsMenu.DropDownItems.Remove(toMenuTrayTotp);
            toMenuTrayTotp.Dispose();
            m_host.MainWindow.ToolsMenu.DropDownItems.Remove(toSubMenuSettings);
            toSubMenuSettings.Dispose();
            m_host.MainWindow.ToolsMenu.DropDownItems.Remove(toSubMenuSeperator1);
            toSubMenuSeperator1.Dispose();
            m_host.MainWindow.ToolsMenu.DropDownItems.Remove(toSubMenuAbout);
            toSubMenuAbout.Dispose();

            //Remove Notify Icon menus.
            m_host.MainWindow.MainNotifyIcon.ContextMenuStrip.Opening -= OnNotifyMenuOpening;
            m_host.MainWindow.MainNotifyIcon.ContextMenuStrip.Items.Remove(niMenuTitle);
            niMenuTitle.Dispose();
            foreach (var Menu in niMenuList)
            {
                m_host.MainWindow.MainNotifyIcon.ContextMenuStrip.Items.Remove(Menu);
                Menu.Dispose();
            }
            m_host.MainWindow.MainNotifyIcon.ContextMenuStrip.Items.Remove(niMenuSeperator);
            niMenuSeperator.Dispose();

            //Remove Context menus.
            m_host.MainWindow.EntryContextMenu.Opening -= OnEntryMenuOpening;
            m_host.MainWindow.EntryContextMenu.Items.Remove(enMenuCopyTotp);
            enMenuCopyTotp.Dispose();

            //Unregister auto-type function.
            if (SprEngine.FilterPlaceholderHints.Contains(m_host.CustomConfig.GetString(setname_string_AutoType_FieldName, setdef_string_AutoType_FieldName).ExtWithBrackets()))
            {
                SprEngine.FilterCompile -= SprEngine_FilterCompile;
                SprEngine.FilterPlaceholderHints.Remove(m_host.CustomConfig.GetString(setname_string_AutoType_FieldName, setdef_string_AutoType_FieldName).ExtWithBrackets());
            }

            //Remove Column provider.
            m_host.ColumnProviderPool.Remove(liColumnTotp);
            liColumnTotp = null;

            //Remove Timer.
            liRefreshTimer.Tick -= OnTimerTick;
            liRefreshTimer.Dispose();
        }

        /// <summary>
        /// Returns the image of the plugin to display in the KeePass plugin list.
        /// </summary>
        public override Image SmallIcon
        {
            get { return Properties.Resources.TOTP; }
        }

        /// <summary>
        /// Returns update URL for KeepAss automatic update check. (file must be UTF-8 without BOM (support for BOM fron KP 2.21))
        /// </summary>
        public override string UpdateUrl
        {
            get { return "http://gartech.byethost32.com/version_manifest.txt"; }
        }
    }
}