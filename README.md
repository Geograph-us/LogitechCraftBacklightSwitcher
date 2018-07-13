# LogitechCraftBacklightSwitcher
Easy back-light switcher for Logitech Craft Keyboard.

LogitechOptions must be installed in your system.

This program just toggle backlight checkbox in LogitechOptions. For it using [White Framework](https://github.com/TestStack/White) - C# UI Automation Framework.

**Command line:**
- On - turn on backlight
- Off - turn off backlight
- [empty] - toggle backlight

  
**settings.ini**
- LogiOptionsUiExe = %ProgramData%\Logishrd\LogiOptions\Software\Current\LogiOptionsUI.exe
- LogiOptionsUiArguments = devid:6b350
- CheckBoxAutomationId = BacklightingCheckBox
- MainWindowName = Logitech Options
- Timeout = 10000

