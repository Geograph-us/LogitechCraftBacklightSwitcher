using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using White.Core;
using White.Core.Configuration;
using White.Core.Factory;
using White.Core.UIItems;
using White.Core.UIItems.Finders;
using White.Core.UIItems.WindowItems;

namespace LogitechCraftBacklightSwitcher
{
    class Program
    {
        static string AppPath = AppDomain.CurrentDomain.BaseDirectory;
        static string IniFileName = AppPath + "settings.ini";

        static string LogiOptionsUiExe = @"%ProgramData%\Logishrd\LogiOptions\Software\Current\LogiOptionsUI.exe";
        static string LogiOptionsUiArguments = "devid:6b350";
        static string CheckBoxAutomationId = "BacklightingCheckBox";
        static string MainWindowName = "Logitech Options";
        static int Timeout = 15000;

        static int Main(string[] args)
        {
            Logger.InitLogger();
            Logger.Log.Info($"Started. Args:[{String.Join(" ", args)}]");

            var iniFile = new IniFile(IniFileName);
            LogiOptionsUiExe = iniFile.GetValue("Settings", "LogiOptionsUiExe", LogiOptionsUiExe);
            LogiOptionsUiArguments = iniFile.GetValue("Settings", "LogiOptionsUiArguments", LogiOptionsUiArguments);
            CheckBoxAutomationId = iniFile.GetValue("Settings", "CheckBoxAutomationId", CheckBoxAutomationId);
            MainWindowName = iniFile.GetValue("Settings", "MainWindowName", MainWindowName);
            Timeout = iniFile.GetValue("Settings", "Timeout", Timeout);

            LogiOptionsUiExe = Regex.Replace(LogiOptionsUiExe, "%(.*?)%",
                (m) =>
                {
                    return Environment.GetEnvironmentVariable(m.Result("$1"));
                });

            if (!File.Exists(LogiOptionsUiExe))
            {
                Logger.Log.Error($@"File {LogiOptionsUiExe} Not Found!");
                return 1;
            }

            var startInfo = new ProcessStartInfo(LogiOptionsUiExe) { Arguments = LogiOptionsUiArguments };
            startInfo.WindowStyle = ProcessWindowStyle.Minimized;
            try
            {
                CoreAppXmlConfiguration.Instance.BusyTimeout = Timeout;
                using (var application = Application.AttachOrLaunch(startInfo))
                {
                    using (var mainWindow = application.GetWindow(SearchCriteria.ByText(MainWindowName), InitializeOption.NoCache))
                    {
                        mainWindow.DisplayState = DisplayState.Minimized;

                        var timeOutInMs = Timeout;
                        var timer = new Stopwatch();
                        timer.Start();
                        CheckBox checkBox = null;

                        while (checkBox == null && timer.ElapsedMilliseconds < timeOutInMs)
                        {
                            checkBox = mainWindow.Get<CheckBox>(SearchCriteria.ByAutomationId(CheckBoxAutomationId));
                        }

                        if (args.Length > 0 && String.Equals(args[0], "on", StringComparison.OrdinalIgnoreCase))
                        {
                            Logger.Log.Info("Turn On Requested");
                            if (checkBox.Checked) checkBox.Toggle();
                        }
                        else if (args.Length > 0 && String.Equals(args[0], "off", StringComparison.OrdinalIgnoreCase))
                        {
                            Logger.Log.Info("Turn Off Requested");
                            if (!checkBox.Checked) checkBox.Toggle();
                        }
                        else
                        {
                            Logger.Log.Info("Toggle Requested");
                            checkBox.Toggle();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Fatal(ex.ToString());
                return 2;
            }

            Logger.Log.Info("Done");
            return 0;
        }
    }
}
