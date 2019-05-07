# KeePass2-TrayTOTP

Time based One Time Password Plugin to enable 2 step authentication (supports Google and Dropbox) by generating TOTPs from the KeePass tray icon. This plugin also provides a custom column to display and/or generate TOTP's. Auto-Type is also supported when the specified custom field is used in a keystroke sequence.
This plugin requires KeePass 2.21 as it uses features that are not available in earlier versions.

This repository is forked from the original at: https://sourceforge.net/projects/traytotp-kp2/

## Notes
To build this plugin from scratch, simply use the `build.bat` file to compile the plugin and output the `TrayTOTP.plgx` to the same folder.  Please ensure that you have the latest version of the `KeePass.exe` file in the root folder of this repository before building.

